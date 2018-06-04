using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CarActor
{
    public class RouteJson
    {
        [JsonProperty("vehicles")]
        public List<RoutePointJson> RoutePoints { get; } = new List<RoutePointJson>();
    }
}
