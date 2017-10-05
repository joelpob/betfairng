using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace BetfairNG.Data
{
    public class MarketOnCloseOrder
    {
        [JsonProperty(PropertyName = "liability")]
        public double Liability { get; set; }

        public override string ToString()
        {
            return new StringBuilder()
                        .AppendFormat("liability={0}", Liability)
                        .ToString();
        }
    }
}
