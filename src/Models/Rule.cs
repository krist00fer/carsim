using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Models
{
    public class Rule
    {
        public string VehicleId { get; set; }
        public string GeoBoundaryJson { get; set; }
        public int MaxSpeed { get; set; }
    }
}
