using Microsoft.KernelMemory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SemanticKernelConntectorOllama.Models
{
    public class OllamaConfig
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public enum TextGenerationTypes
        {
            Auto,
            TextCompletion,
            Chat
        }

        //
        // Summary:
        //     The type of OpenAI completion to use, either Text (legacy) or Chat. When using
        //     Auto, the client uses OpenAI model names to detect the correct protocol. Most
        //     OpenAI models use Chat. If you're using a non-OpenAI model, you might want to
        //     set this manually.
        public TextGenerationTypes TextGenerationType { get; set; }

        //
        // Summary:
        //     OpenAI HTTP endpoint. You may need to override this to work with OpenAI compatible
        //     services like LM Studio.
        public string Endpoint { get; set; } = "";


        //
        // Summary:
        //     Model used for text generation. Chat models can be used too.
        public string TextModel { get; set; } = string.Empty;


        //
        // Summary:
        //     The max number of tokens supported by the text model.
        public int TextModelMaxTokenTotal { get; set; } = 8192;


        //
        // Summary:
        //     Model used to embedding generation/
        public string EmbeddingModel { get; set; } = string.Empty;


        //
        // Summary:
        //     The max number of tokens supported by the embedding model. Default to OpenAI
        //     ADA2 settings.
        public int EmbeddingModelMaxTokenTotal { get; set; } = 8191;


        //
        // Summary:
        //     The number of dimensions output embeddings should have. Only supported in "text-embedding-3"
        //     and later models developed with MRL, see https://arxiv.org/abs/2205.13147
        public int? EmbeddingDimensions { get; set; }

        //
        // Summary:
        //     Per documentation the max value is 2048, however, the default is lower (100)
        //     to avoid sending too many tokens and being throttled. You can increase the value
        //     in your local configuration if needed. See https://platform.openai.com/docs/api-reference/embeddings/create.
        public int MaxEmbeddingBatchSize { get; set; } = 100;


        //
        // Summary:
        //     How many times to retry in case of throttling.
        public int MaxRetries { get; set; } = 10;
    }
}
