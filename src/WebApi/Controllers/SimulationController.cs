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
        // POST api/simulation
        [HttpPost]
        public async void Post([FromBody] SimulatedCar car)
        {
            var proxy = ActorProxy.Create<ICarActor>(new ActorId($"SimulatedCar:{car.VehicleId}"));
            if (car.Running)
                await proxy.StartAsync(car.VehicleId, car.StartLatitude, car.StartLongitude, CancellationToken.None);
            else
                await proxy.StopAsync(CancellationToken.None);
        }

        // PUT api/simulation/5
        [HttpPut("{vehicleId}")]
        public async void Put(string vehicleId, [FromBody]SimulatedCar car)
        {
            var proxy = ActorProxy.Create<ICarActor>(new ActorId($"SimulatedCar:{car.VehicleId}"));
            if (car.Running)
                await proxy.StartAsync(car.VehicleId, car.StartLatitude, car.StartLongitude, CancellationToken.None);
            else
                await proxy.StopAsync(CancellationToken.None);
        }

        // DELETE api/simulation/5
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
