using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SemanticKernelConntectorOllama.Models;
using Microsoft.KernelMemory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.KernelMemory.AI;

namespace SemanticKernelConntectorOllama
{
    public class OllamaTextEmbeddingGenerationService : ITextEmbeddingGenerationService, ITextEmbeddingGenerator, ITextEmbeddingBatchGenerator
    {
        private readonly string _apiUrl;
        private readonly string _model;
        private readonly ILogger<OllamaTextEmbeddingGenerationService> _logger;
        private readonly int _maxTokens;
        private readonly int _maxBatchSize;

        public OllamaTextEmbeddingGenerationService(string apiUrl, string model, ILoggerFactory? loggerFactory = null)
        {
            _apiUrl = apiUrl ?? throw new ArgumentNullException(nameof(apiUrl));
            _model = model ?? throw new ArgumentNullException(nameof(model));
            _logger = loggerFactory?.CreateLogger<OllamaTextEmbeddingGenerationService>() ?? NullLogger<OllamaTextEmbeddingGenerationService>.Instance;
            _maxTokens = 8191; // Adjust based on model requirements
            _maxBatchSize = 10; // Adjust based on your requirements
        }

        public OllamaTextEmbeddingGenerationService(OllamaConfig config, ILoggerFactory? loggerFactory = null)
        {
            _apiUrl = config.Endpoint ?? throw new ArgumentNullException(nameof(config.Endpoint));
            _model = config.EmbeddingModel ?? throw new ArgumentNullException(nameof(config.EmbeddingModel));
            _logger = loggerFactory?.CreateLogger<OllamaTextEmbeddingGenerationService>() ?? NullLogger<OllamaTextEmbeddingGenerationService>.Instance;
            _maxTokens = config.EmbeddingModelMaxTokenTotal;
            _maxBatchSize = config.MaxEmbeddingBatchSize;
        }

        public int MaxTokens => _maxTokens;

        public int MaxBatchSize => _maxBatchSize;

        public IReadOnlyDictionary<string, object?> Attributes => throw new NotImplementedException();

        public async Task<Embedding> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default)
        {
            _logger.LogTrace("Generating embedding, model '{0}'", _model);
            using var httpClient = new HttpClient();
            var requestBody = new { model = _model, prompt = text };
            var content = new StringContent(JsonSerializer.Serialize(requestBody), System.Text.Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(_apiUrl + "/api/embeddings", content, cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var embeddingResponse = JsonSerializer.Deserialize<EmbeddingResponseModel>(responseContent);

            if (embeddingResponse == null || embeddingResponse.Embedding == null)
            {
                throw new InvalidOperationException("Invalid response from embedding service.");
            }

            return new Embedding(new ReadOnlyMemory<float>(embeddingResponse.Embedding));
        }

        public async Task<Embedding[]> GenerateEmbeddingBatchAsync(IEnumerable<string> textList, CancellationToken cancellationToken = default)
        {
            List<string> list = textList.ToList();
            _logger.LogTrace("Generating embeddings, model '{0}', batch size '{1}'", _model, list.Count);
            var embeddingTasks = new List<Task<Embedding>>();

            foreach (var text in list)
            {
                embeddingTasks.Add(GenerateEmbeddingAsync(text, cancellationToken));
            }

            return await Task.WhenAll(embeddingTasks);
        }

        public async Task<IList<ReadOnlyMemory<float>>> GenerateEmbeddingsAsync(IList<string> data, Kernel? kernel = null, CancellationToken cancellationToken = default)
        {
            var embeddings = new List<ReadOnlyMemory<float>>();

            foreach (var text in data)
            {
                try
                {
                    var embedding = await GenerateEmbeddingAsync(text, cancellationToken);
                    embeddings.Add(embedding.Data);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error generating embedding for text '{text}'");
                }
            }

            return embeddings;
        }

        public int CountTokens(string text)
        {
            // Implement your logic for counting tokens in the text
            return text.Split(' ').Length;
        }

        public IReadOnlyList<string> GetTokens(string text)
        {
            // Implement your logic for tokenizing the text
            return text.Split(' ');
        }
    }
}
