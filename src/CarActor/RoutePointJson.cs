using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CarActor
{
    public class RoutePointJson
    {
        [JsonProperty("vehicle_id")]
        public int VehicleId { get; set; }
        [JsonProperty("time_stamp")]
        public long Timestamp { get; set; }
        [JsonProperty("alarm")]
        public int? Alarm { get; set; }
        [JsonProperty("altitude")]
        public double? Altitude { get; set; }
        [JsonProperty("latitude")]
        public double? Latitude { get; set; }
        [JsonProperty("longitude")]
        public double? Longitude { get; set; }
        [JsonProperty("direction")]
        public double? Direction { get; set; }
        [JsonProperty("battery")]
        public int? BatteryLevel { get; set; }
        [JsonProperty("ignition")]
        public bool? Ignition { get; set; }
        [JsonProperty("speed")]
        public double? Speed { get; set; }
#if false
            "pwr_ext": 12430,
            "status": 0,
            "time_stamp": 1528095539,
            "vehicle_id": "10249"
#endif
    }
}
