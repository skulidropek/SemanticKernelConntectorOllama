using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SemanticKernelConntectorOllama.Models
{
    internal class TextGenerationResponseModel
    {
        [JsonPropertyName("model")]
        public string Model { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("response")]
        public string Response { get; set; }

        [JsonPropertyName("done")]
        public bool Done { get; set; }

        [JsonPropertyName("done_reason")]
        public string DoneReason { get; set; }

        [JsonPropertyName("context")]
        public List<int> Context { get; set; }
    }
}
