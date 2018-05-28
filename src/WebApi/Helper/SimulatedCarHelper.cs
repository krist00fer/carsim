﻿using CarActor.Interfaces;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Actors.Query;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace WebApi.Helper
{
    public static class SimulatedCarHelper
    {
        public static async Task<IEnumerable<Vehicle>> GetVehicles()
        {
            ContinuationToken continuationToken = null;
            var vehicles = new List<Vehicle>();
            var actorServiceProxy = ActorServiceProxy.Create(new Uri("fabric:/MyActorApp/MyActorService"), null);
            var queriedActorCount = 0;
            do
            {
                var queryResult = actorServiceProxy.GetActorsAsync(continuationToken, CancellationToken.None).GetAwaiter().GetResult();
                foreach (var actorInformation in queryResult.Items)
                {
                    //var proxy = GetActorProxy(actorInformation.ActorId);
                    //vehicles.Add(await proxy.Get());
                    vehicles.Add(new Vehicle { VehicleId = actorInformation.ActorId.GetStringId().Substring(13) });
                }
                queriedActorCount += queryResult.Items.Count();
                continuationToken = queryResult.ContinuationToken;
            } while (continuationToken != null);
            return vehicles;
        }

        public static async Task<Vehicle> GetVehicle(string vehicleId)
        {
            //var proxy = GetActorProxy(vehicleId);
            //return proxy.Get();
            return new Vehicle { VehicleId = vehicleId };
        }

        public static async Task<VehicleStatus> GetVehicleStatus(string vehicleId)
        {
            var proxy = GetActorProxy(vehicleId);
            return proxy.GetStatusAsync(CancellationToken.None);
        }


        public static ActorId GetActorId(string vehicleId)
        {
            return new ActorId($"SimulatedCar:{vehicleId}");
        }

        public static ICarActor GetActorProxy(string vehicleId)
        {
            return GetActorProxy(GetActorId(vehicleId));
        }

        public static ICarActor GetActorProxy(ActorId actorId)
        {
            return ActorProxy.Create<ICarActor>(actorId);
        }
    }
}