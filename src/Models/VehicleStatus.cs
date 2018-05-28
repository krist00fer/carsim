using System;
using System.Collections.Generic;
using System.Text;

namespace Models
{
    public class VehicleStatus
    {
        //public string VehicleId { get; set; }
        //public double Latitude { get; set; }
        //public double Longitude { get; set; }
        //public double Direction { get; set; }
        //public double Speed { get; set; }
        //public DateTime Date { get; set; }
        public int FromGeoPosition { get; set; }
        public int CurrentGeoPosition { get; set; }
        public int ToGeoPosition { get; set; }
    }
}
