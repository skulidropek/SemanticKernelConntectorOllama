using Microsoft.SemanticKernel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using Microsoft.KernelMemory;
using Microsoft.SemanticKernel.Embeddings;
using SemanticKernelConntectorOllama.Models;
using Microsoft.KernelMemory.AI;
using Microsoft.SemanticKernel.TextGeneration;

namespace SemanticKernelConntectorOllama
{
    public static class KernelBuilderExtensions
    {
        public static IKernelMemoryBuilder WithOllamaDefaults(this IKernelMemoryBuilder builder, OllamaConfig config, ILoggerFactory? loggerFactory = null, bool onlyForRetrieval = false, HttpClient? httpClient = null)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (config == null) throw new ArgumentNullException(nameof(config));

            builder.Services.AddOllamaTextEmbeddingGenerationService(config);
            builder.Services.AddOllamaTextGenerationService(config.Endpoint, config.TextModel);

            if (!onlyForRetrieval)
            {
                builder.AddIngestionEmbeddingGenerator(new OllamaTextEmbeddingGenerationService(config, loggerFactory));
            }

            return builder;
        }

        public static IKernelBuilder AddOllamaTextEmbeddingGenerationService(this IKernelBuilder builder, OllamaConfig config)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (config == null) throw new ArgumentNullException(nameof(config));

            builder.Services.AddSingleton<ITextEmbeddingGenerationService>(provider =>
                new OllamaTextEmbeddingGenerationService(config));
            builder.Services.AddSingleton<ITextEmbeddingGenerator>(provider =>
                new OllamaTextEmbeddingGenerationService(config));

            return builder;
        }

        public static IServiceCollection AddOllamaTextEmbeddingGenerationService(this IServiceCollection services, OllamaConfig config)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (config == null) throw new ArgumentNullException(nameof(config));

            services.AddSingleton<ITextEmbeddingGenerationService>(provider =>
                new OllamaTextEmbeddingGenerationService(config));
            services.AddSingleton<ITextEmbeddingGenerator>(provider =>
                new OllamaTextEmbeddingGenerationService(config));

            return services;
        }

        public static IKernelBuilder AddOllamaTextEmbeddingGenerationService(this IKernelBuilder builder, string apiUrl, string model)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (string.IsNullOrEmpty(apiUrl)) throw new ArgumentException("API URL cannot be null or empty.", nameof(apiUrl));
            if (string.IsNullOrEmpty(model)) throw new ArgumentException("Model cannot be null or empty.", nameof(model));

            builder.Services.AddSingleton<ITextEmbeddingGenerationService>(provider =>
                new OllamaTextEmbeddingGenerationService(apiUrl, model));

            return builder;
        }

        public static IServiceCollection AddOllamaTextEmbeddingGenerationService(this IServiceCollection services, string apiUrl, string model)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (string.IsNullOrEmpty(apiUrl)) throw new ArgumentException("API URL cannot be null or empty.", nameof(apiUrl));
            if (string.IsNullOrEmpty(model)) throw new ArgumentException("Model cannot be null or empty.", nameof(model));

            services.AddSingleton<ITextEmbeddingGenerationService>(provider =>
                new OllamaTextEmbeddingGenerationService(apiUrl, model));

            return services;
        }

        public static IServiceCollection AddOllamaTextGenerationService(
           this IServiceCollection services,
           string apiUrl,
           string model)
        {
            // Register the OllamaTextGenerationService in the DI container
            services.AddSingleton<ITextGenerator>(provider =>
            {
                // Create and configure the OllamaTextGenerationService instance
                var loggerFactory = provider.GetService<ILoggerFactory>();
                return new OllamaTextGenerationService(apiUrl, model, loggerFactory);
            });

            services.AddSingleton<ITextGenerationService>(provider =>
            {
                // Create and configure the OllamaTextGenerationService instance
                var loggerFactory = provider.GetService<ILoggerFactory>();
                return new OllamaTextGenerationService(apiUrl, model, loggerFactory);
            });

            return services;
        }
        public static IKernelBuilder AddOllamaTextGenerationService(
            this IKernelBuilder builder,
            string apiUrl,
            string model)
        {
            // Register the OllamaTextGenerationService in the DI container
            builder.Services.AddSingleton<ITextGenerator>(provider =>
            {
                // Create and configure the OllamaTextGenerationService instance
                var loggerFactory = provider.GetService<ILoggerFactory>();
                return new OllamaTextGenerationService(apiUrl, model, loggerFactory);
            });

            builder.Services.AddSingleton<ITextGenerationService>(provider =>
            {
                // Create and configure the OllamaTextGenerationService instance
                var loggerFactory = provider.GetService<ILoggerFactory>();
                return new OllamaTextGenerationService(apiUrl, model, loggerFactory);
            });

            return builder;
        }
    }
}
