using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BetfairNG.Data
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum RaceStatus
    {
        DORMANT,
        DELAYED,
        PARADING,
        GOINGDOWN,
        GOINGBEHIND,
        APPROACHING,
        GOINGINTRAPS,
        HARERUNNING,
        ATTHEPOST,
        OFF,
        FINISHED,
        FINALRESULT,
        FALSESTART,
        PHOTOGRAPH,
        RESULT,
        WEIGHEDIN,
        RACEVOID,
        NORACE,
        RERUN,
        ABANDONED,
    }
}