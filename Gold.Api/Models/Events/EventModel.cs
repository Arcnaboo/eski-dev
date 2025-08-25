using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
namespace Gold.Api.Models.Events
{
    public class EventModel
    {
        [JsonProperty("type")]
        public string EventType { get; set; } // "wedding" or "gold_day" or "special"
        
        [JsonProperty("event")]
        public object EventObject { get; set; }
    }
}
