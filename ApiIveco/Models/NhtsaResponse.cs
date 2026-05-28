using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ApiIveco.Models
{
    public class NhtsaResponse
    {
        [JsonPropertyName("Results")]
        public List<NhtsaVariable> Results { get; set; }
    }

    public class NhtsaVariable
    {
        [JsonPropertyName("Variable")]
        public string Variable { get; set; }

        [JsonPropertyName("Value")]
        public string Value { get; set; }
    }
}