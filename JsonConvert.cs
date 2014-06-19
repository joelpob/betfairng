using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;

namespace BetfairNG
{
    public class JsonConvert
    {
        public static JsonResponse<T> Import<T>(TextReader reader)
        {
            var jsonResponse = reader.ReadToEnd();
            return Deserialize<JsonResponse<T>>(jsonResponse);
        }

        public static T Deserialize<T>(string json)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
        }

        public static void Export(JsonRequest request, TextWriter writer)
        {
            var json = Serialize<JsonRequest>(request);
            writer.Write(json);
        }

        public static string Serialize<T>(T value)
        {
            var settings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
            return Newtonsoft.Json.JsonConvert.SerializeObject(value, settings);
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class JsonRequest
    {
        public JsonRequest()
        {
            JsonRpc = "2.0";
        }

        [JsonProperty(PropertyName = "jsonrpc", NullValueHandling = NullValueHandling.Ignore)]
        public string JsonRpc { get; set; }

        [JsonProperty(PropertyName = "method")]
        public string Method { get; set; }

        [JsonProperty(PropertyName = "params")]
        public object Params { get; set; }

        [JsonProperty(PropertyName = "id")]
        public object Id { get; set; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class JsonResponse<T>
    {
        [JsonProperty(PropertyName = "jsonrpc", NullValueHandling = NullValueHandling.Ignore)]
        public string JsonRpc { get; set; }

        [JsonProperty(PropertyName = "result", NullValueHandling = NullValueHandling.Ignore)]
        public T Result { get; set; }

        [JsonProperty(PropertyName = "error", NullValueHandling = NullValueHandling.Ignore)]
        public Data.Exceptions.Exception Error { get; set; }

        [JsonProperty(PropertyName = "id")]
        public object Id { get; set; }

        [JsonIgnore]
        public bool HasError
        {
            get { return Error != null; }
        }
    }
}
