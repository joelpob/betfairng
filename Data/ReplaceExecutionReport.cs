using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace BetfairNG.Data
{
    public class ReplaceExecutionReport
    {
        [JsonProperty(PropertyName = "customerRef")]
        public string CustomerRef { get; set; }

        [JsonProperty(PropertyName = "status")]
        public ExecutionReportStatus Status { get; set; }

        [JsonProperty(PropertyName = "errorCode")]
        public ExecutionReportErrorCode ErrorCode { get; set; }

        [JsonProperty(PropertyName = "marketId")]
        public string MarketId { get; set; }

        [JsonProperty(PropertyName = "instructionReports")]
        public IList<ReplaceInstructionReport> InstructionReports { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder()
                        .AppendFormat("{0}", "ReplaceExecutionReport")
                        .AppendFormat(" : Status={0}", Status)
                        .AppendFormat(" : ErrorCode={0}", ErrorCode)
                        .AppendFormat(" : MarketId={0}", MarketId)
                        .AppendFormat(" : CustomerRef={0}", CustomerRef);

            if (InstructionReports != null && InstructionReports.Count > 0)
            {
                int idx = 0;
                foreach (var instructionReport in InstructionReports)
                {
                    sb.AppendFormat(" : InstructionReport[{0}]={{{1}}}", idx++, instructionReport);
                }
            }

            return sb.ToString();
        }
    }
}
