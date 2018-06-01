using CarActor.Interfaces;
using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
        private VehicleStatus _vehicleStatus;
        private double? _maxSpeed;
        private GeographicPosition[] _points;

        /// <summary>
        /// Initializes a new instance of CarActor
        /// </summary>
        /// <param name="actorService">The Microsoft.ServiceFabric.Actors.Runtime.ActorService that will host this actor instance.</param>
        /// <param name="actorId">The Microsoft.ServiceFabric.Actors.ActorId for this actor instance.</param>
        public CarActor(ActorService actorService, ActorId actorId) 
            : base(actorService, actorId)
        {
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
            double newSpeed = (1600.0 * (double)speedPercent) / 100.0;

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
            await StateManager.SetStateAsync("vehicleStatus", _vehicleStatus);
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

            // register remainder
            await RegisterReminderAsync(
                "update-reminder", null,
                TimeSpan.FromSeconds(5),    //The amount of time to delay before firing the reminder
                TimeSpan.FromSeconds(5));    //The time interval between firing of reminders
        }

        public async Task StopAsync(CancellationToken cancellation)
        {
            var reminder = GetReminder("update-reminder");
            await UnregisterReminderAsync(reminder);
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
                foreach (var polygon in polygons.Coordinates)
                {
                    foreach (var point in polygon.Coordinates)
                    {
                        var geoPosition = point as GeoJSON.Net.Geometry.GeographicPosition;
                        points.Add(geoPosition);
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

#if false
public bool IsPointInPolygon( Point p, Point[] polygon )
{
    double minX = polygon[ 0 ].X;
    double maxX = polygon[ 0 ].X;
    double minY = polygon[ 0 ].Y;
    double maxY = polygon[ 0 ].Y;
    for ( int i = 1 ; i < polygon.Length ; i++ )
    {
        Point q = polygon[ i ];
        minX = Math.Min( q.X, minX );
        maxX = Math.Max( q.X, maxX );
        minY = Math.Min( q.Y, minY );
        maxY = Math.Max( q.Y, maxY );
    }

    if ( p.X < minX || p.X > maxX || p.Y < minY || p.Y > maxY )
    {
        return false;
    }

    // http://www.ecse.rpi.edu/Homepages/wrf/Research/Short_Notes/pnpoly.html
    bool inside = false;
    for ( int i = 0, j = polygon.Length - 1 ; i < polygon.Length ; j = i++ )
    {
        if ( ( polygon[ i ].Y > p.Y ) != ( polygon[ j ].Y > p.Y ) &&
             p.X < ( polygon[ j ].X - polygon[ i ].X ) * ( p.Y - polygon[ i ].Y ) / ( polygon[ j ].Y - polygon[ i ].Y ) + polygon[ i ].X )
        {
            inside = !inside;
        }
    }

    return inside;
}
#endif
    }
}
