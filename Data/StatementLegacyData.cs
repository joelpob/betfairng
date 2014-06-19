using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace BetfairNG.Data
{
    public class StatementLegacyData 
    {
        [JsonProperty(PropertyName = "avgPrice")]
        public double AveragePrice { get; set; }

        [JsonProperty(PropertyName = "betSize")]
        public double BetSize { get; set; }

        [JsonProperty(PropertyName = "betType")]
        public string BetType { get; set; }

        [JsonProperty(PropertyName = "betCategoryType")]
        public string BetCategoryType { get; set; }

        [JsonProperty(PropertyName = "commissionRate")]
        public string CommissionRate { get; set; }

        [JsonProperty(PropertyName = "eventId")]
        public long EventId { get; set; }

        [JsonProperty(PropertyName = "eventTypeId")]
        public long EventTypeId { get; set; }

        [JsonProperty(PropertyName = "fullMarketName")]
        public string FullMarketName { get; set; }

        [JsonProperty(PropertyName = "grossBetAmount")]
        public double GrossBetAmount { get; set; }

        [JsonProperty(PropertyName = "marketName")]
        public string MarketName { get; set; }

        [JsonProperty(PropertyName = "marketType")]
        public string MarketType { get; set; }

        [JsonProperty(PropertyName = "placedDate")]
        public DateTime PlacedDate { get; set; }

        [JsonProperty(PropertyName = "selectionId")]
        public long SelectionId { get; set; }

        [JsonProperty(PropertyName = "selectionName")]
        public string SelectionName { get; set; }

        [JsonProperty(PropertyName = "startDate")]
        public DateTime StartDate { get; set; }

        [JsonProperty(PropertyName = "transactionType")]
        public string TransactionType { get; set; }

        [JsonProperty(PropertyName = "transactionId")]
        public long TransactionId { get; set; }

        [JsonProperty(PropertyName = "winLose")]
        public string WinLose { get; set; }
    }
}
