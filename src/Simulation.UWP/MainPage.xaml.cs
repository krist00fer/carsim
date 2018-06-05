using Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Newtonsoft.Json;
using System.Text;
using WebApi.Models;
using Windows.UI.Xaml.Controls.Maps;
using GeoJSON.Net.Geometry;
using GeoJSON.Net.Feature;
using Windows.UI;
using Windows.Devices.Geolocation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Simulation.UWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        //private string _endpoint = "http://pb-lynkdemosf.westeurope.cloudapp.azure.com:8961";
        private string _endpoint = "http://localhost:8961";
        private MapIcon _currentPosition;
        //private string _geoBoundaryJson = "{ \"type\": \"FeatureCollection\", \"features\": [{ \"type\": \"Feature\",\"properties\": {}, \"geometry\": {\"type\": \"Polygon\", \"coordinates\": [[[11.877833, 57.681415], [11.904169, 57.689031], [11.896745, 57.693944], [11.89998, 57.700291], [11.878627, 57.695647], [11.886423, 57.689573], [11.877833, 57.681415]]]}}]}";
        private string _geoBoundaryJson = "{ \"type\": \"FeatureCollection\", \"features\": [{ \"type\": \"Feature\",\"properties\": {}, \"geometry\": {\"type\": \"Polygon\", \"coordinates\": [[[11.850157582337527,57.763200915637526], [11.877522417662473,57.763200915637526], [11.877522417662473,57.79056575096247], [11.850157582337527,57.79056575096247], [11.850157582337527,57.763200915637526]]]}}]}";

        public MainPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var position = new Windows.Devices.Geolocation.Geopoint(new Windows.Devices.Geolocation.BasicGeoposition() { Latitude = 57.693826, Longitude = 11.891392 });
            base.OnNavigatedTo(e);
            mapControl.Center = position;
            mapControl.ZoomLevel = 14;
            _currentPosition = new MapIcon { Location = position, Title = "Current Position", ZIndex = 0 };
            mapControl.MapElements.Add(_currentPosition);

            var points = ParseGeoBoundaryJson(_geoBoundaryJson);
            var polygon = new MapPolygon();
            polygon.FillColor = Colors.Transparent;
            polygon.StrokeColor = Colors.Green;
            polygon.StrokeThickness = 5;

            var pathPositions = new List<BasicGeoposition>();
            foreach (var point in points)
            {
                pathPositions.Add(new BasicGeoposition() { Latitude = point.Latitude, Longitude = point.Longitude });
            }
            polygon.Paths.Add(new Geopath(pathPositions));
            mapControl.MapElements.Add(polygon);

            var timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(2);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private async void Timer_Tick(object sender, object e)
        {
            try
            {
                var responseVehicleStatus = new HttpClient().GetAsync(new Uri(_endpoint + "/api/vehicles/UAK298/status")).Result;
                if (responseVehicleStatus.IsSuccessStatusCode)
                {
                    var vehicleStatus = JsonConvert.DeserializeObject<VehicleStatus>(await responseVehicleStatus.Content.ReadAsStringAsync());
                    var newPosition = new Windows.Devices.Geolocation.Geopoint(new Windows.Devices.Geolocation.BasicGeoposition() { Latitude = vehicleStatus.Latitude, Longitude = vehicleStatus.Longitude });
                    //mapControl.Center = position;
                    _currentPosition.Location = newPosition;
                }

                var responseRuleStatus = new HttpClient().GetAsync(new Uri(_endpoint + "/api/vehicles/UAK298/rulestatus")).Result;
                if (responseRuleStatus.IsSuccessStatusCode)
                {
                    var ruleStatusJson = JsonConvert.DeserializeObject<bool?>(await responseRuleStatus.Content.ReadAsStringAsync());
                    ruleStatus.Text = (!ruleStatusJson.HasValue ? "UNKNOWN" : ruleStatusJson.Value ? "INSIDE" : "OUTSIDE");
                    ruleStatus.Foreground = (!ruleStatusJson.HasValue ? new SolidColorBrush(Colors.Yellow) : ruleStatusJson.Value ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Red));
                }
            }
            catch (Exception)
            {
            }
        }

        private void startButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                var responseRule = new HttpClient().PostAsync(new Uri(_endpoint + "/api/rules"),
                                    new StringContent(JsonConvert.SerializeObject(
                                        new Rule() { VehicleId = "UAK298", MaxSpeed = 80, GeoBoundaryJson = _geoBoundaryJson }),
                                        Encoding.UTF8, "application/json")).Result;
                var responseSimulation = new HttpClient().PostAsync(new Uri(_endpoint + "/api/simulation"),
                                    new StringContent(JsonConvert.SerializeObject(
                                        new SimulatedCar() { VehicleId = "UAK298", Running = true, StartLatitude = mapControl.Center.Position.Latitude, StartLongitude = mapControl.Center.Position.Longitude }),
                                        Encoding.UTF8, "application/json")).Result;
            }
            catch (Exception)
            {
                return;
            }

            startButton.IsEnabled = false;
            stopButton.IsEnabled = true;
        }

        private void stopButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                var responseSimulation = new HttpClient().PostAsync(new Uri(_endpoint + "/api/simulation"),
                                    new StringContent(JsonConvert.SerializeObject(
                                        new SimulatedCar() { VehicleId = "UAK298", Running = false, StartLatitude = mapControl.Center.Position.Latitude, StartLongitude = mapControl.Center.Position.Longitude }),
                                        Encoding.UTF8, "application/json")).Result;
            }
            catch (Exception)
            {
                return;
            }

            startButton.IsEnabled = true;
            stopButton.IsEnabled = false;
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
    }
}
