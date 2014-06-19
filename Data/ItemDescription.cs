using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BetfairNG.Data
{
    public class ItemDescription
    {
        [JsonProperty(PropertyName = "eventTypeDesc")]
        public string EventTypeDesc { get; set; }

        [JsonProperty(PropertyName = "eventDesc")]
        public string EventDesc { get; set; }

        [JsonProperty(PropertyName = "marketDesc")]
        public string MarketDesc { get; set; }

        [JsonProperty(PropertyName = "marketStartTime")]
        public DateTime MarketStartTime { get; set; }

        [JsonProperty(PropertyName = "runnerDesc")]
        public string RunnerDesc { get; set; }

        [JsonProperty(PropertyName = "numberOfWinners")]
        public int NumberOfWinners { get; set; }
    }
}
