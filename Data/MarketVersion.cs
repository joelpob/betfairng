using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Newtonsoft.Json;

namespace BetfairNG.Data
{
    public class MarketVersion
    {
        [JsonProperty(PropertyName = "version")]
        public long Version { get; set; }
    }
}
