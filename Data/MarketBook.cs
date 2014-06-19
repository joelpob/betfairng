using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace BetfairNG.Data
{
    public class MarketBook
    {
        public MarketBook()
        {
            this.Created = DateTime.Now;
        }

        [JsonProperty(PropertyName = "marketId")]
        public string MarketId { get; set; }

        [JsonProperty(PropertyName = "isMarketDataDelayed")]
        public bool IsMarketDataDelayed { get; set; }

        [JsonProperty(PropertyName = "status")]
        public MarketStatus Status { get; set; }

        [JsonProperty(PropertyName = "betDelay")]
        public int BetDelay { get; set; }

        [JsonProperty(PropertyName = "bspReconciled")]
        public bool IsBspReconciled { get; set; }

        [JsonProperty(PropertyName = "complete")]
        public bool IsComplete { get; set; }

        [JsonProperty(PropertyName = "inplay")]
        public bool IsInplay { get; set; }

        [JsonProperty(PropertyName = "numberOfWinners")]
        public int NumberOfWinners { get; set; }

        [JsonProperty(PropertyName = "numberOfRunners")]
        public int NumberOfRunners { get; set; }

        [JsonProperty(PropertyName = "numberOfActiveRunners")]
        public int NumberOfActiveRunners { get; set; }

        [JsonProperty(PropertyName = "lastMatchTime")]
        public DateTime? LastMatchTime { get; set; }

        [JsonProperty(PropertyName = "totalMatched")]
        public double TotalMatched { get; set; }

        [JsonProperty(PropertyName = "totalAvailable")]
        public double TotalAvailable { get; set; }

        [JsonProperty(PropertyName = "crossMatching")]
        public bool IsCrossMatching { get; set; }

        [JsonProperty(PropertyName = "runnersVoidable")]
        public bool IsRunnersVoidable { get; set; }

        [JsonProperty(PropertyName = "version")]
        public long Version { get; set; }

        [JsonProperty(PropertyName = "runners")]
        public List<Runner> Runners { get; set; }

        public DateTime Created { get; set; }
        public int DbId { get; set; }
    }
}
