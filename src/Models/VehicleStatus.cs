using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Models
{
    [DataContract]
    public class VehicleStatus
    {
        [DataMember]
        public string VehicleId { get; set; }
        [DataMember]
        public double Latitude { get; set; }
        [DataMember]
        public double Longitude { get; set; }
        [DataMember]
        public double Direction { get; set; }
        [DataMember]
        public double Speed { get; set; }
        [DataMember]
        public DateTime Date { get; set; }
    }
}
