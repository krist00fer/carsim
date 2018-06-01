using CarActor.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using WebApi.Models;

namespace WebApi.Controllers
{
    [Produces("application/json")]
    [Route("api/Rules")]
    public class RulesController : Controller
    {
        // POST api/simulation
        [HttpPost]
        public async void Post([FromBody] Rule rule)
        {
            var proxy = ActorProxy.Create<ICarActor>(new ActorId($"SimulatedCar:{rule.VehicleId}"));
            await proxy.SetRuleAsync(rule.MaxSpeed, rule.GeoBoundaryJson);
        }

        // PUT api/simulation/5
        [HttpPut("{vehicleId}")]
        public async void Put(string vehicleId, [FromBody]Rule rule)
        {
            var proxy = ActorProxy.Create<ICarActor>(new ActorId($"SimulatedCar:{vehicleId}"));
            await proxy.SetRuleAsync(rule.MaxSpeed, rule.GeoBoundaryJson);
        }
    }
}