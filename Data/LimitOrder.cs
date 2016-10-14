using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace BetfairNG.Data
{
    public class LimitOrder
    {
        [JsonProperty(PropertyName = "size")]
        public double Size { get; set; }

        [JsonProperty(PropertyName = "price")]
        public double Price { get; set; }

        [JsonProperty(PropertyName = "persistenceType")]
        public PersistenceType PersistenceType { get; set; }

        [JsonProperty(PropertyName = "timeInForce")]
        public TimeInForce? TimeInForce { get; set; }

        [JsonProperty(PropertyName = "minFillSize")]
        public double? MinFillSize { get; set; }

        [JsonProperty(PropertyName = "betTargetType")]
        public BetTargetType? BetTargetType { get; set; }

        [JsonProperty(PropertyName = "betTargetSize")]
        public double? BetTargetSize { get; set; }

        public override string ToString()
        {
            return new StringBuilder()
                        .AppendFormat("Size={0}", Size)
                        .AppendFormat(" : Price={0}", Price)
                        .AppendFormat(" : PersistenceType={0}", PersistenceType)
                        .ToString();
        }
    }
}
