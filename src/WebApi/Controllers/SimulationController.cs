using CarActor.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    public class SimulationController
    {
        // POST api/vehicles
        [HttpPost]
        public async void Post([FromBody]SimulatedCar value)
        {
            // TODO: Add new actor
            var proxy = ActorProxy.Create<ICarActor>(new ActorId($"SimulatedCar:{value.VehicleId}"));
            if (value.Running)
                await proxy.Start();
            else
                await proxy.Stop();
        }

        // PUT api/vehicle/5
        [HttpPut("{vehicleId}")]
        public async void Put(string vehicleId, [FromBody]SimulatedCar value)
        {
            var proxy = ActorProxy.Create<ICarActor>(new ActorId($"SimulatedCar:{value.VehicleId}"));
            if (value.Running)
                await proxy.Start();
            else
                await proxy.Stop();
        }

        // DELETE api/values/5
        [HttpDelete("{vehicleId}")]
        public async void Delete(string vehicleId)
        {
            var actorId = new ActorId($"SimulatedCar:{vehicleId}");
            var proxy = ActorProxy.Create<ICarActor>(actorId);
            await proxy.Stop();
            var serviceProxy = ActorServiceProxy.Create(new Uri("TODO"), actorId);
            await serviceProxy.DeleteActorAsync(actorId, CancellationToken.None);
        }
    }
}
