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
            var proxy = ActorProxy.Create<ICarActor>(new ActorId($"SimulatedCar:{value.VehicleId}"));
            if (value.Running)
                await proxy.StartAsync(value.FromPosition, value.ToPosition, CancellationToken.None);
            else
                await proxy.StopAsync(CancellationToken.None);
        }

        // PUT api/vehicle/5
        [HttpPut("{vehicleId}")]
        public async void Put(string vehicleId, [FromBody]SimulatedCar value)
        {
            var proxy = ActorProxy.Create<ICarActor>(new ActorId($"SimulatedCar:{value.VehicleId}"));
            if (value.Running)
                await proxy.StartAsync(value.FromPosition, value.ToPosition, CancellationToken.None);
            else
                await proxy.StopAsync(CancellationToken.None);
        }

        // DELETE api/values/5
        [HttpDelete("{vehicleId}")]
        public async void Delete(string vehicleId)
        {
            var actorId = new ActorId($"SimulatedCar:{vehicleId}");
            var proxy = ActorProxy.Create<ICarActor>(actorId);
            await proxy.StopAsync(CancellationToken.None);
            var serviceProxy = ActorServiceProxy.Create(new Uri("fabric:/CarActorSF/CarActorService"), actorId);
            await serviceProxy.DeleteActorAsync(actorId, CancellationToken.None);
        }
    }
}
