// Copyright (c) Microsoft. All rights reserved.

using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.VectorData;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;

namespace Core.Utilities.Config;

/// <summary>
/// This class containes classes required to achieve Semantic Caching with Filters.
/// <see cref="IPromptRenderFilter"/> is used to get rendered prompt and check in cache if similar prompt was already answered.
/// If there is a record in cache, then previously cached answer will be returned to the user instead of making a call to LLM.
/// If there is no record in cache, a call to LLM will be performed, and result will be cached together with rendered prompt.
/// <see cref="IFunctionInvocationFilter"/> is used to update cache with rendered prompt and related LLM result.
/// </summary>
public class KernelWithCacheProvider
{
    private static ILogger<FunctionCacheFilter> logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<FunctionCacheFilter>();

    #region Configuration

    /// <summary>
    /// Returns <see cref="Kernel"/> instance with required registered services.
    /// </summary>
    public static Kernel GetKernelWithCache(Action<IServiceCollection> configureVectorStore)
    {
        IKernelBuilder builder = KernelBuilderProvider.CreateKernelWithChatCompletion();

        // Add vector store for caching purposes (e.g. in-memory, Redis, Azure Cosmos DB)
        configureVectorStore(builder.Services);

        // Add prompt render filter to query cache and check if rendered prompt was already answered.
        builder.Services.AddSingleton<IPromptRenderFilter, PromptCacheFilter>();

        // Add function invocation filter to cache rendered prompts and LLM results.
        builder.Services.AddSingleton<IFunctionInvocationFilter, FunctionCacheFilter>();
        //builder.Services.AddLogging(services => services.AddConsole().SetMinimumLevel(LogLevel.Trace));

        return builder.Build();
    }

    #endregion

    #region Cache Filters

    /// <summary>
    /// Base class for filters that contains common constant values.
    /// </summary>
    public class CacheBaseFilter
    {
        /// <summary>
        /// Collection/table name in cache to use.
        /// </summary>
        protected const string CollectionName = "llm_responses";

        /// <summary>
        /// Metadata key in function result for cache record id, which is used to overwrite previously cached response.
        /// </summary>
        protected const string RecordIdKey = "CacheRecordId";
    }

    /// <summary>
    /// Filter which is executed during prompt rendering operation.
    /// </summary>
    public sealed class PromptCacheFilter(
        ITextEmbeddingGenerationService textEmbeddingGenerationService,
        IVectorStore vectorStore)
        : CacheBaseFilter, IPromptRenderFilter
    {
        public async Task OnPromptRenderAsync(PromptRenderContext context, Func<PromptRenderContext, Task> next)
        {
            // Trigger prompt rendering operation
            await next(context);

            // Get rendered prompt
            var prompt = context.RenderedPrompt!;

            var promptEmbedding = await textEmbeddingGenerationService.GenerateEmbeddingAsync(prompt);

            var collection = vectorStore.GetCollection<string, CacheRecord>(CollectionName);
            await collection.CreateCollectionIfNotExistsAsync();

            // Search for similar prompts in cache.
            var searchResults = await collection.VectorizedSearchAsync(promptEmbedding, new() { Top = 1 }, context.CancellationToken);
            var searchResult = (await searchResults.Results.FirstOrDefaultAsync())?.Record;

            // If result exists, return it.
            if (searchResult is not null)
            {
                // Override function result. This will prevent calling LLM and will return result immediately.
                context.Result = new FunctionResult(context.Function, searchResult.Result)
                {
                    Metadata = new Dictionary<string, object?> { [RecordIdKey] = searchResult.Id }
                };
            }
        }
    }

    /// <summary>
    /// Filter which is executed during function invocation.
    /// </summary>
    public sealed class FunctionCacheFilter(
        ITextEmbeddingGenerationService textEmbeddingGenerationService,
        IVectorStore vectorStore)
        : CacheBaseFilter, IFunctionInvocationFilter
    {
        public async Task OnFunctionInvocationAsync(FunctionInvocationContext context, Func<FunctionInvocationContext, Task> next)
        {
            logger.LogDebug("> OnFunctionInvocationAsync");

            // Trigger function invocation
            await next(context);

            // Get function invocation result
            var result = context.Result;

            // If there was any rendered prompt, cache it together with LLM result for future calls.
            if (!string.IsNullOrEmpty(context.Result.RenderedPrompt))
            {
                // Get cache record id if result was cached previously or generate new id.
                var recordId = context.Result.Metadata?.GetValueOrDefault(RecordIdKey, Guid.NewGuid().ToString()) as string;

                // Generate prompt embedding.
                var promptEmbedding = await textEmbeddingGenerationService.GenerateEmbeddingAsync(context.Result.RenderedPrompt);

                // Cache rendered prompt and LLM result.
                var collection = vectorStore.GetCollection<string, CacheRecord>(CollectionName);
                await collection.CreateCollectionIfNotExistsAsync();

                var cacheRecord = new CacheRecord
                {
                    Id = recordId!,
                    Prompt = context.Result.RenderedPrompt,
                    Result = result.ToString(),
                    PromptEmbedding = promptEmbedding
                };

                await collection.UpsertAsync(cacheRecord, cancellationToken: context.CancellationToken);
            }
        }
    }

    #endregion

    #region Vector Store Record

    private sealed class CacheRecord
    {
        [VectorStoreRecordKey]
        public string Id { get; set; }

        [VectorStoreRecordData]
        public string Prompt { get; set; }

        [VectorStoreRecordData]
        public string Result { get; set; }

        [VectorStoreRecordVector(Dimensions: 1536)]
        public ReadOnlyMemory<float> PromptEmbedding { get; set; }
    }

    #endregion
}