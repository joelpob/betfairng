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
    public enum OrderBy
    {
        BY_BET,
        BY_MARKET,
        BY_MATCH_TIME,
        BY_PLACE_TIME,
        BY_SETTLED_TIME,
        BY_VOID_TIME,
    }
}
