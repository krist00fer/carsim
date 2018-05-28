using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Models
{
    [DataContract]
    public class VehicleStatus
    {
        //public string VehicleId { get; set; }
        //public double Latitude { get; set; }
        //public double Longitude { get; set; }
        //public double Direction { get; set; }
        //public double Speed { get; set; }
        //public DateTime Date { get; set; }
        [DataMember]
        public int FromGeoPosition { get; set; }
        [DataMember]
        public int CurrentGeoPosition { get; set; }
        [DataMember]
        public int ToGeoPosition { get; set; }
    }
}
