using Microsoft.KernelMemory.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using SemanticKernelConntectorOllama.Models;
using System.Runtime.CompilerServices;
using Microsoft.SemanticKernel.TextGeneration;
using Microsoft.SemanticKernel;

namespace SemanticKernelConntectorOllama
{
    public class OllamaTextGenerationService : ITextGenerator, ITextGenerationService
    {
        private readonly string _apiUrl;
        private readonly string _model;
        private readonly ILogger<OllamaTextGenerationService> _logger;
        private readonly int _maxTokens;

        public OllamaTextGenerationService(string apiUrl, string model, ILoggerFactory? loggerFactory = null)
        {
            _apiUrl = apiUrl ?? throw new ArgumentNullException(nameof(apiUrl));
            _model = model ?? throw new ArgumentNullException(nameof(model));
            _logger = loggerFactory?.CreateLogger<OllamaTextGenerationService>() ?? NullLogger<OllamaTextGenerationService>.Instance;
            _maxTokens = 8191; // Adjust based on model requirements
        }

        public int MaxTokenTotal => _maxTokens;

        public IReadOnlyDictionary<string, object?> Attributes => throw new NotImplementedException();

        public async IAsyncEnumerable<string> GenerateTextAsync(string prompt, TextGenerationOptions options, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            _logger.LogTrace("Generating text with model '{0}'", _model);

            using var httpClient = new HttpClient();
            var requestBody = new
            {
                model = _model,
                prompt,
                max_tokens = options.MaxTokens // Используем options для других параметров
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), System.Text.Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync($"{_apiUrl}/api/generate", content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Request failed with status code {0}", response.StatusCode);
                throw new HttpRequestException($"Request failed with status code {response.StatusCode}");
            }

            await using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var reader = new StreamReader(responseStream);

            var jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() }
            };

            string? line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                TextGenerationResponseModel? generationResponse = null;
                try
                {
                    // Предполагаем, что каждый фрагмент ответа - это отдельная строка JSON.
                    generationResponse = JsonSerializer.Deserialize<TextGenerationResponseModel>(line, jsonSerializerOptions);
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Failed to deserialize response content");
                    throw new InvalidOperationException("Failed to deserialize response content", ex);
                }

                if (generationResponse != null && !string.IsNullOrEmpty(generationResponse.Response))
                {
                    yield return generationResponse.Response;
                }
            }
        }

        public int CountTokens(string text)
        {
            // Простая реализация для подсчета количества токенов
            return text.Split(' ').Length;
        }

        public IReadOnlyList<string> GetTokens(string text)
        {
            // Простая реализация для токенизации текста
            return text.Split(' ');
        }

        public async Task<IReadOnlyList<TextContent>> GetTextContentsAsync(string prompt, PromptExecutionSettings? executionSettings = null, Kernel? kernel = null, CancellationToken cancellationToken = default)
        {
            _logger.LogTrace("Getting text contents with model '{0}'", _model);

            var texts = new List<TextContent>();

            await foreach (var generatedText in GenerateTextAsync(prompt, new TextGenerationOptions { MaxTokens = _maxTokens }, cancellationToken))
            {
                // Предположим, что у TextContent есть конструктор, который принимает строку
                texts.Add(new TextContent(generatedText, _model));
            }

            return texts;
        }

        public async IAsyncEnumerable<StreamingTextContent> GetStreamingTextContentsAsync(string prompt, PromptExecutionSettings? executionSettings = null, Kernel? kernel = null, CancellationToken cancellationToken = default)
        {
            _logger.LogTrace("Getting streaming text contents with model '{0}'", _model);

            await foreach (var generatedText in GenerateTextAsync(prompt, new TextGenerationOptions { MaxTokens = _maxTokens }, cancellationToken))
            {
                // Предположим, что у StreamingTextContent есть конструктор, который принимает строку
                yield return new StreamingTextContent(generatedText, 0, _model, null, null, null);
            }
        }
    }
}
