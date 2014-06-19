using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace BetfairNG.Data
{
    public class CurrentOrderSummary
    {
        [JsonProperty(PropertyName = "betId")]
        public string BetId { get; set; }

        [JsonProperty(PropertyName = "marketId")]
        public string MarketId { get; set; }

        [JsonProperty(PropertyName = "selectionId")]
        public long SelectionId { get; set; }

        [JsonProperty(PropertyName = "handicap")]
        public double Handicap { get; set; }

        [JsonProperty(PropertyName = "priceSize")]
        public PriceSize PriceSize { get; set; }

        [JsonProperty(PropertyName = "bspLiability")]
        public double BspLiability { get; set; }

        [JsonProperty(PropertyName = "side")]
        public Side Side { get; set; }

        [JsonProperty(PropertyName = "status")]
        public OrderStatus Status { get; set; }

        [JsonProperty(PropertyName = "persistenceType")]
        public PersistenceType PersistenceType { get; set; }

        [JsonProperty(PropertyName = "orderType")]
        public OrderType OrderType { get; set; }

        [JsonProperty(PropertyName = "placedDate")]
        public DateTime PlacedDate { get; set; }

        [JsonProperty(PropertyName = "matchedDate")]
        public DateTime MatchedDate { get; set; }

        [JsonProperty(PropertyName = "averagePriceMatched")]
        public double AveragePriceMatched { get; set; }

        [JsonProperty(PropertyName = "sizeMatched")]
        public double SizeMatched { get; set; }

        [JsonProperty(PropertyName = "sizeRemaining")]
        public double SizeRemaining { get; set; }

        [JsonProperty(PropertyName = "sizeLapsed")]
        public double SizeLapsed { get; set; }

        [JsonProperty(PropertyName = "sizeCancelled")]
        public double SizeCancelled { get; set; }

        [JsonProperty(PropertyName = "sizeVoided")]
        public double SizeVoided { get; set; }

        [JsonProperty(PropertyName = "regulatorAuthCode")]
        public string RegulatorAuthCode { get; set; }

        [JsonProperty(PropertyName = "regulatorCode")]
        public string RegulatorCode { get; set; }
    }
}
