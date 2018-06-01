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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Simulation.UWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            mapControl.Center = new Windows.Devices.Geolocation.Geopoint(new Windows.Devices.Geolocation.BasicGeoposition() { Latitude = 57.693826, Longitude = 11.891392 });
            mapControl.ZoomLevel = 14;

            var timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(2);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private async void Timer_Tick(object sender, object e)
        {
            try
            {
                var responseVehicleStatus = new HttpClient().PostAsync(new Uri("http://localhost:8961/api/vehicles/UAK298/status"), null).Result;
                if (responseVehicleStatus.IsSuccessStatusCode)
                {
                    var vehicleStatus = JsonConvert.DeserializeObject<VehicleStatus>(await responseVehicleStatus.Content.ReadAsStringAsync());
                    mapControl.Center = new Windows.Devices.Geolocation.Geopoint(new Windows.Devices.Geolocation.BasicGeoposition() { Latitude = vehicleStatus.Latitude, Longitude = vehicleStatus.Longitude });
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
                var responseSimulation = new HttpClient().PostAsync(new Uri("http://localhost:8961/api/simulation"),
                                    new StringContent(JsonConvert.SerializeObject(
                                        new SimulatedCar() { VehicleId = "UAK298", Running = true, StartLatitude = mapControl.Center.Position.Latitude, StartLongitude = mapControl.Center.Position.Longitude }),
                                        Encoding.UTF8, "application/json")).Result;
                var responseRule = new HttpClient().PostAsync(new Uri("http://localhost:8961/api/rules"),
                                    new StringContent(JsonConvert.SerializeObject(
                                        new Rule() { VehicleId = "UAK298", MaxSpeed = 80, GeoBoundaryJson = "{ \"type\": \"FeatureCollection\", \"features\": [{ \"type\": \"Feature\",\"properties\": {}, \"geometry\": {\"type\": \"Polygon\", \"coordinates\": [[[11.877833, 57.681415], [11.904169, 57.689031], [11.896745, 57.693944], [11.89998, 57.700291], [11.878627, 57.695647], [11.886423, 57.689573], [11.877833, 57.681415]]]}}]}" }),
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
                var responseSimulation = new HttpClient().PostAsync(new Uri("http://localhost:8961/api/simulation"),
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
    }
}
