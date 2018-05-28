﻿using Microsoft.AspNetCore.Mvc;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApi.Helper;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    public class VehiclesController
    {
        // GET api/vehicles
        [HttpGet]
        public Task<IEnumerable<Vehicle>> Get()
        {
            return SimulatedCarHelper.GetVehicles();
        }

        // GET api/vehicles/5
        [HttpGet("{vehicleId}")]
        public Task<Vehicle> Get(string vehicleId)
        {
            // TODO: Return specific vehicle
            return SimulatedCarHelper.GetVehicle(vehicleId);
        }

        // GET api/vehicle/UAK298/status
        [HttpGet("{vehicleId}/status")]
        public VehicleStatus GetStatus(string vehicleId)
        {
            // TODO: return current status for vehicle
            return new VehicleStatus { VehicleId = "UAK298", Latitude = 14, Longitude = 15, Direction = 180, Speed = 90, Date = DateTime.Now };
        }
        // POST api/vehicles
        [HttpPost]
        public void Post([FromBody]Vehicle value)
        {
            // TODO: Add new vehicle
        }

        // PUT api/vehicle/5
        [HttpPut("{vehicleId}")]
        public void Put(string vehicleId, [FromBody]string value)
        {
            // TODO: Update new vehicle
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
