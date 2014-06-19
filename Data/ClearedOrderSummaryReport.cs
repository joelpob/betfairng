using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace BetfairNG.Data
{
    public class ClearedOrderSummaryReport
    {
        [JsonProperty(PropertyName = "clearedOrders")]
        public IList<ClearedOrderSummary> ClearedOrders { get; set; }

        [JsonProperty(PropertyName = "moreAvailable")]
        public bool MoreAvailable { get; set; }
    }
}
