using Models;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using WebApi.Models;

namespace SimulationConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var responseSimulation = new HttpClient().PostAsync(new Uri("http://localhost:8961/api/simulation"), 
                                new StringContent(JsonConvert.SerializeObject(
                                    new SimulatedCar() { VehicleId = "UAK298", Running = true, StartLatitude = 57.693826, StartLongitude = 11.891392 }), 
                                    Encoding.UTF8, "application/json")).Result;
            var responseRule = new HttpClient().PostAsync(new Uri("http://localhost:8961/api/rules"),
                                new StringContent(JsonConvert.SerializeObject(
                                    new Rule() { VehicleId = "UAK298", MaxSpeed = 80, GeoBoundaryJson = "{ \"type\": \"FeatureCollection\", \"features\": [{ \"type\": \"Feature\",\"properties\": {}, \"geometry\": {\"type\": \"Polygon\", \"coordinates\": [[[11.877833, 57.681415], [11.904169, 57.689031], [11.896745, 57.693944], [11.89998, 57.700291], [11.878627, 57.695647], [11.886423, 57.689573], [11.877833, 57.681415]]]}}]}" }),
                                    Encoding.UTF8, "application/json")).Result;
        }
    }
}
