using Models;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;

namespace SimulationConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            SimulatedCar car = new SimulatedCar() { VehicleId = "UAK298", Running = true, FromPosition = 0, ToPosition = 20 };
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, new Uri("http://localhost:8961/api/simulation"));
            request.Headers.Add("content", "application/json");
            request.Content = new StringContent(JsonConvert.SerializeObject(car), Encoding.UTF8, "application/json"); ;
            var response = client.SendAsync(request).Result;
        }
    }
}
