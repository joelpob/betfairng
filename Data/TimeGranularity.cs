using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BetfairNG.Data
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum TimeGranularity
    {
        DAYS,
        HOURS,
        MINUTES
    }
}
