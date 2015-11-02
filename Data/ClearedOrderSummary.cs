using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace BetfairNG.Data
{
    public class ClearedOrderSummary
    {
        [JsonProperty(PropertyName = "eventTypeId")]
        public string EventTypeId { get; set; }

        [JsonProperty(PropertyName = "eventId")]
        public string EventType { get; set; }

        [JsonProperty(PropertyName = "marketId")]
        public string MarketId { get; set; }

        [JsonProperty(PropertyName = "selectionId")]
        public long SelectionId { get; set; }

        [JsonProperty(PropertyName = "handicap")]
        public double Handicap { get; set; }

        [JsonProperty(PropertyName = "betId")]
        public string BetId { get; set; }

        [JsonProperty(PropertyName = "placedDate")]
        public DateTime PlacedDate { get; set; }

        [JsonProperty(PropertyName = "persistenceType")]
        public PersistenceType PersistenceType { get; set; }

        // TODO:// could be a bug in doco -- should be OrderType enum?
        [JsonProperty(PropertyName = "orderType")]
        public string OrderType { get; set; }

        [JsonProperty(PropertyName = "side")]
        public Side Side { get; set; }

        [JsonProperty(PropertyName = "itemDescription")]
        public ItemDescription ItemDescription { get; set; }

        [JsonProperty(PropertyName = "betOutcome")]
        public string BetOutcome { get; set; }

        [JsonProperty(PropertyName = "priceRequested")]
        public double Price { get; set; }

        [JsonProperty(PropertyName = "settledDate")]
        public DateTime SettledDate { get; set; }
        
        [JsonProperty(PropertyName = "lastMatchedDate")]
        public DateTime LastMatchedDate { get; set; }

        [JsonProperty(PropertyName = "betCount")]
        public int BetCount { get; set; }

        [JsonProperty(PropertyName = "commission")]
        public double Commission { get; set; }

        [JsonProperty(PropertyName = "priceMatched")]
        public double PriceMatched { get; set; }

        [JsonProperty(PropertyName = "priceReduced")]
        public bool PriceReduced { get; set; }

        [JsonProperty(PropertyName = "sizeSettled")]
        public double SizeSettled { get; set; }

        [JsonProperty(PropertyName = "profit")]
        public double Profit { get; set; }

        [JsonProperty(PropertyName = "sizeCancelled")]
        public double SizeCancelled { get; set; }
    }
}
