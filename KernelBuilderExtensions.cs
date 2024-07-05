using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel;
using Microsoft.Extensions.DependencyInjection;

namespace SemanticKernelConntectorOllama
{
    public static class KernelBuilderExtensions
    {
        public static IKernelBuilder AddOllamaTextEmbeddingGenerationService(this IKernelBuilder builder, string apiUrl, string model)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (string.IsNullOrEmpty(apiUrl)) throw new ArgumentException("API URL cannot be null or empty.", nameof(apiUrl));
            if (string.IsNullOrEmpty(model)) throw new ArgumentException("Model cannot be null or empty.", nameof(model));

            builder.Services.AddSingleton<ITextEmbeddingGenerationService>(provider =>
                new OllamaTextEmbeddingGenerationService(apiUrl, model));

            return builder;
        }
    }
}
