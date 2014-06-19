using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetfairNG.Data
{
    public class MarketProfitAndLoss
    {
        [JsonProperty(PropertyName = "marketId")]
        public string MarketId { get; set; }

        [JsonProperty(PropertyName = "commissionApplied")]
        public double CommissionApplied { get; set; }

        [JsonProperty(PropertyName = "profitAndLosses")]
        public List<RunnerProfitAndLoss> ProfitAndLosses { get; set; }
    }
}
