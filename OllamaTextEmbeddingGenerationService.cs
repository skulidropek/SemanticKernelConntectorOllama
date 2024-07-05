using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;
using SemanticKernelConntectorOllama.Models;

namespace SemanticKernelConntectorOllama
{
    internal class OllamaTextEmbeddingGenerationService : ITextEmbeddingGenerationService
    {
        private readonly string _apiUrl;
        private readonly string _model;

        public OllamaTextEmbeddingGenerationService(string apiUrl, string model)
        {
            _apiUrl = apiUrl ?? throw new ArgumentNullException(nameof(apiUrl));
            _model = model ?? throw new ArgumentNullException(nameof(model));
        }

        public IReadOnlyDictionary<string, object?> Attributes => new Dictionary<string, object?>();

        public async Task<IList<ReadOnlyMemory<float>>> GenerateEmbeddingsAsync(IList<string> data, Kernel? kernel = null, CancellationToken cancellationToken = default)
        {
            using var httpClient = new HttpClient();
            var embeddings = new List<ReadOnlyMemory<float>>();

            foreach (var text in data)
            {
                var requestBody = new { model = _model, prompt = text };
                var content = new StringContent(JsonSerializer.Serialize(requestBody), System.Text.Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync(_apiUrl, content, cancellationToken);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                var embeddingResponse = JsonSerializer.Deserialize<EmbeddingResponseModel>(responseContent);

                if (embeddingResponse != null && embeddingResponse.Embedding != null)
                {
                    embeddings.Add(new ReadOnlyMemory<float>(embeddingResponse.Embedding));
                }
            }

            return embeddings;
        }
    }
}
