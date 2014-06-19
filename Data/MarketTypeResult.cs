using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetfairNG.Data
{
    public class MarketTypeResult
    {
        [JsonProperty(PropertyName = "marketType")]
        public string MarketType { get; set; }
        
        [JsonProperty(PropertyName = "marketCount")]
        public int MarketCount { get; set; }
    }
}
