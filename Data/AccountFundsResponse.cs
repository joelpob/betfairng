using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace BetfairNG.Data
{
    public class AccountFundsResponse
    {
        [JsonProperty(PropertyName = "availableToBetBalance")]
        public double AvailableToBetBalance { get; set; }

        [JsonProperty(PropertyName = "exposure")]
        public double Exposure { get; set; }

        [JsonProperty(PropertyName = "retainedCommission")]
        public double RetainedCommission { get; set; }

        [JsonProperty(PropertyName = "exposureLimit")]
        public double ExposureLimit { get; set; }
    }
}
