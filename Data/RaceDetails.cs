using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BetfairNG.Data
{
    public class RaceDetails
    {
        [JsonProperty(PropertyName = "meetingId")]
        public string MeetingId { get; set; }

        [JsonProperty(PropertyName = "raceId")]
        public string RaceId { get; set; }

        [JsonProperty(PropertyName = "raceStatus")]
        public RaceStatus RaceStatus { get; set; }

        [JsonProperty(PropertyName = "lastUpdated")]
        public DateTime LastUpdated { get; set; }

        [JsonProperty(PropertyName = "responseCode")]
        public ResponseCode ResponseCode { get; set; }
    }
}
