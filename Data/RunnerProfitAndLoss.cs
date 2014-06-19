using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetfairNG.Data
{
    public class RunnerProfitAndLoss
    {
        [JsonProperty(PropertyName = "selectionId")]
        public long SelectionId { get; set; }

        [JsonProperty(PropertyName = "ifWin")]
        public double IfWin { get; set; }

        [JsonProperty(PropertyName = "ifLose")]
        public double IfLose { get; set; }
    }
}
