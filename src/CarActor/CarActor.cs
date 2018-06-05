using CarActor.Interfaces;
using GeoCoordinatePortable;
using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;
using Microsoft.Azure.NotificationHubs;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace CarActor
{
    /// <remarks>
    /// This class represents an actor.
    /// Every ActorID maps to an instance of this class.
    /// The StatePersistence attribute determines persistence and replication of actor state:
    ///  - Persisted: State is written to disk and replicated.
    ///  - Volatile: State is kept in memory only and replicated.
    ///  - None: State is kept in memory only and not replicated.
    /// </remarks>
    [StatePersistence(StatePersistence.Persisted)]
    internal class CarActor : Actor, ICarActor, IRemindable
    {
        private static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private RouteJson _route;
        private VehicleStatus _vehicleStatus;
        private double? _maxSpeed;
        private GeographicPosition[] _points;
        bool? _inside;

        /// <summary>
        /// Initializes a new instance of CarActor
        /// </summary>
        /// <param name="actorService">The Microsoft.ServiceFabric.Actors.Runtime.ActorService that will host this actor instance.</param>
        /// <param name="actorId">The Microsoft.ServiceFabric.Actors.ActorId for this actor instance.</param>
        public CarActor(ActorService actorService, ActorId actorId)
            : base(actorService, actorId)
        {
            if (_route == null)
            {
                _route = LoadRoute(actorService.Context.CodePackageActivationContext.GetCodePackageObject("Code").Path);
                _route.RoutePoints.Reverse();
            }
        }

        /// <summary>
        /// This method is called whenever an actor is activated.
        /// An actor is activated the first time any of its methods are invoked.
        /// </summary>
        protected override async Task OnActivateAsync()
        {
            ActorEventSource.Current.ActorMessage(this, "Actor activated.");
            var status = await StateManager.TryGetStateAsync<VehicleStatus>("vehicleStatus");
            _vehicleStatus = status.HasValue ? status.Value : null;
            var maxSpeed = await StateManager.TryGetStateAsync<double>("maxspeed");
            _maxSpeed = maxSpeed.HasValue ? maxSpeed.Value : (double?)null;
            var geoBoundaryJson = await StateManager.TryGetStateAsync<string>("geoBoundaryJson");
            _points = geoBoundaryJson.HasValue ? ParseGeoBoundaryJson(geoBoundaryJson.Value) : null;
        }

        public Task<VehicleStatus> GetStatusAsync()
        {
            return Task.FromResult(_vehicleStatus);
        }

        public Task<bool?> GetRuleStatusAsync()
        {
            if (_vehicleStatus != null)
            {
                var point = new GeographicPosition(_vehicleStatus.Latitude, _vehicleStatus.Longitude);
                return Task.FromResult<bool?>(IsPointInPolygon(point, _points));
            }
            else
                return Task.FromResult((bool?)null);
        }

        public async Task ReceiveReminderAsync(string reminderName, byte[] state, TimeSpan dueTime, TimeSpan period)
        {
            var speedPercent = new Random((int)DateTime.Now.Ticks).Next(100);
            double newSpeed = (100.0 * (double)speedPercent) / 100.0;

            // assign new speed
            _vehicleStatus.Speed = newSpeed;
            // calculate new position
            double distanceInMeters = ((newSpeed * 1000.0) / 3600.0) * period.TotalSeconds;
            // given the starting position, direction and distance travelled - we can calculate new position
            var dx = distanceInMeters * Math.Cos(_vehicleStatus.Direction);
            var dy = distanceInMeters * Math.Sin(_vehicleStatus.Direction);
            var deltaLongitude = dx / (111320.0 * Math.Cos(_vehicleStatus.Latitude));
            var deltaLatitude = dy / 110540.0;
            _vehicleStatus.Latitude += deltaLatitude;
            _vehicleStatus.Longitude += deltaLongitude;

            await SaveNewPoint();
        }

        public async Task StartAsync(string vehicleId, double latitude, double longitude, CancellationToken cancellationToken)
        {
            if (_vehicleStatus == null)
                _vehicleStatus = new VehicleStatus()
                {
                    Latitude = latitude,
                    Longitude = longitude,
                };
            _vehicleStatus.VehicleId = vehicleId;
            _vehicleStatus.Speed = 0;
            _vehicleStatus.Date = DateTime.UtcNow;
            _vehicleStatus.Direction = new Random((int)DateTime.Now.Ticks).Next(360);
            // save starting vechicle status
            await StateManager.SetStateAsync("vehicleStatus", _vehicleStatus);

            if (_route != null)
            {
                long currentTick = 0;
                Task.Run(async () =>
                {
                    int routePointIndex = 0;
                    while (routePointIndex < _route.RoutePoints.Count)
                    {
                        while (!_route.RoutePoints[routePointIndex].Latitude.HasValue ||
                                !_route.RoutePoints[routePointIndex].Longitude.HasValue)
                            routePointIndex++;

                        var routePoint = _route.RoutePoints[routePointIndex];
                        if (routePoint.Latitude.Value != _vehicleStatus.Latitude || routePoint.Longitude.Value != _vehicleStatus.Longitude)
                        {
                            var milliseconds = (currentTick != 0 && routePoint.Timestamp > currentTick ? (FromUnixTime(routePoint.Timestamp) - FromUnixTime(currentTick)).TotalMilliseconds : 0);
                            await Task.Delay((int)milliseconds);
                            currentTick = _route.RoutePoints[routePointIndex].Timestamp;
                            // calculate new speed and direction
                            var sCoord = new GeoCoordinate(_vehicleStatus.Latitude, _vehicleStatus.Longitude);
                            var eCoord = new GeoCoordinate(routePoint.Latitude.Value, routePoint.Longitude.Value);
                            double distanceInMeters = sCoord.GetDistanceTo(eCoord);
                            var newSpeed = (milliseconds > 0 ? (distanceInMeters / (milliseconds / 1000.0)) * 3.6 : 0.0);
                            // assign new speed
                            _vehicleStatus.Speed = newSpeed;
                            _vehicleStatus.Latitude = routePoint.Latitude.Value;
                            _vehicleStatus.Longitude = routePoint.Longitude.Value;

                            await SaveNewPoint();
                        }
                        routePointIndex++;
                    }
                });
            }
            else
            {
                // register remainder
                await RegisterReminderAsync(
                    "update-reminder", null,
                    TimeSpan.FromSeconds(5),    //The amount of time to delay before firing the reminder
                    TimeSpan.FromSeconds(5));    //The time interval between firing of reminders
            }
        }

        public async Task StopAsync(CancellationToken cancellation)
        {
            if (_route == null)
            {
                var reminder = GetReminder("update-reminder");
                await UnregisterReminderAsync(reminder);
            }
        }

        public async Task SetRuleAsync(double maxSpeed, string geoBoundaryJson)
        {
            await StateManager.SetStateAsync("maxSpeed", maxSpeed);
            await StateManager.SetStateAsync("geoBoundaryJson", geoBoundaryJson);
            if (!String.IsNullOrEmpty(geoBoundaryJson))
                _points = ParseGeoBoundaryJson(geoBoundaryJson);
        }

        private GeographicPosition[] ParseGeoBoundaryJson(string geoBoundaryJson)
        {
            var featureCollection = JsonConvert.DeserializeObject<FeatureCollection>(geoBoundaryJson);
            var points = new List<GeographicPosition>();
            foreach (var feature in featureCollection.Features)
            {
                var polygons = feature.Geometry as Polygon;
                if (polygons != null)
                {
                    foreach (var polygon in polygons.Coordinates)
                    {
                        foreach (var point in polygon.Coordinates)
                        {
                            var geoPosition = point as GeoJSON.Net.Geometry.GeographicPosition;
                            points.Add(geoPosition);
                        }
                    }
                }
            }
            return points.ToArray();
        }

        public bool IsPointInPolygon(GeographicPosition p, GeographicPosition[] polygon)
        {
            double minLongitude = polygon[0].Longitude;
            double maxLongitude = polygon[0].Longitude;
            double minLatitude = polygon[0].Latitude;
            double maxLatitude = polygon[0].Latitude;
            for (int i = 1; i < polygon.Length; i++)
            {
                GeographicPosition q = polygon[i];
                minLongitude = Math.Min(q.Longitude, minLongitude);
                maxLongitude = Math.Max(q.Longitude, maxLongitude);
                minLatitude = Math.Min(q.Latitude, minLatitude);
                maxLatitude = Math.Max(q.Latitude, maxLatitude);
            }

            if (p.Longitude < minLongitude || p.Longitude > maxLongitude || p.Latitude < minLatitude || p.Latitude > maxLatitude)
            {
                return false;
            }

            // http://www.ecse.rpi.edu/Homepages/wrf/Research/Short_Notes/pnpoly.html
            bool inside = false;
            for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++)
            {
                if ((polygon[i].Latitude > p.Latitude) != (polygon[j].Latitude > p.Latitude) &&
                     p.Longitude < (polygon[j].Longitude - polygon[i].Longitude) * (p.Latitude - polygon[i].Latitude) / (polygon[j].Latitude - polygon[i].Latitude) + polygon[i].Longitude)
                {
                    inside = !inside;
                }
            }

            return inside;
        }

        private async Task SendNotification(string vehicleId, string message)
        {
            Microsoft.Azure.NotificationHubs.NotificationOutcome outcome = null;
            HttpStatusCode ret = HttpStatusCode.InternalServerError;

            // Android
            var notif = "{ \"data\" : {\"message\":\"" + "From " + vehicleId + ": " + message + "\"}}";
            outcome = await Notifications.Instance.Hub.SendGcmNativeNotificationAsync(notif);
            if (outcome != null)
            {
                if (!((outcome.State == Microsoft.Azure.NotificationHubs.NotificationOutcomeState.Abandoned) ||
                    (outcome.State == Microsoft.Azure.NotificationHubs.NotificationOutcomeState.Unknown)))
                {
                    ret = HttpStatusCode.OK;
                }
            }
        }

        private RouteJson LoadRoute(string path)
        {
            return JsonConvert.DeserializeObject<RouteJson>(File.ReadAllText(Path.Combine(path, "vehicle_tess_iop_poc_se.json")));
        }

        private async Task SaveNewPoint()
        {
            await StateManager.SetStateAsync("vehicleStatus", _vehicleStatus);

            var point = new GeographicPosition(_vehicleStatus.Latitude, _vehicleStatus.Longitude);
            bool newInside = IsPointInPolygon(point, _points);
            if (_inside.HasValue && _inside.Value != newInside)
            {
                await SendNotification(_vehicleStatus.VehicleId, newInside ? "Now we are inside the safe block" : "DANGER: We are outside the safe block!");
            }
            _inside = newInside;
        }

        public static DateTime FromUnixTime(long unixTime)
        {
            return epoch.AddSeconds(unixTime);
        }

    }
}
