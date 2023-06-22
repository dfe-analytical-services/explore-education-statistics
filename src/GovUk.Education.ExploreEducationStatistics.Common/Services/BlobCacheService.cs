#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Cache.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services
{
    public class BlobCacheService : IBlobCacheService
    {
        private readonly IBlobStorageService _blobStorageService;
        private readonly ILogger<BlobCacheService> _logger;
        private readonly StaleCacheWorkflow<BlobCacheKeyAndType, BlobCacheService> _staleWorkflow;

        public BlobCacheService(IBlobStorageService blobStorageService,
            ILogger<BlobCacheService> logger)
        {
            _blobStorageService = blobStorageService;
            _logger = logger;
            _staleWorkflow = new StaleCacheWorkflow<BlobCacheKeyAndType, BlobCacheService>(
                cacheKey => GetItemAsync(cacheKey.CacheKey, cacheKey.Type),
                cacheKey => GetItemMetaAsync(cacheKey.CacheKey),
                (cacheKey, item) => SetItemAsync(cacheKey.CacheKey, item),
                _logger
            );
        }

        // TODO DW - remove???
        public object? GetItem(IBlobCacheKey cacheKey, Type targetType)
        {
            throw new NotImplementedException();
        }

        public async Task<object?> GetItemAsync(IBlobCacheKey cacheKey, Type targetType)
        {
            var blobContainer = cacheKey.Container;
            var key = cacheKey.Key;

            // Attempt to read blob from the storage container
            try
            {
                var result = await _blobStorageService.GetDeserializedJson(cacheKey.Container, cacheKey.Key, targetType)
                    .OrElse(() => (object?) null);

                _logger.LogDebug("Blob cache {HitOrMiss} - for key {CacheKey}",
                    result != null ? "hit" : "miss", key);

                return result;
            }
            catch (JsonException e)
            {
                // If there's an error deserializing the blob, we should
                // assume it's not salvageable and delete it so that it's re-built.
                _logger.LogWarning(e, $"Error deserializing JSON for blobContainer {blobContainer} and cache " +
                                      $"key {key} - deleting cached JSON");
                await _blobStorageService.DeleteBlob(blobContainer, key);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Caught error fetching cache entry from: {BlobContainer}/{Key}", blobContainer, key);
            }

            return default;
        }

        public Task<object?> GetOrCreateAndCacheItemAsync(IBlobCacheKey cacheKey, Type targetType, Func<Task<object?>> createItemFunc)
        {
            return _staleWorkflow.GetOrCreateAndCacheItemAsync(cacheKey, createItemFunc);
        }

        public async Task<CacheItemMeta?> GetItemMetaAsync(IBlobCacheKey cacheKey)
        {
            var blobContainer = cacheKey.Container;
            var key = cacheKey.Key;

            // Attempt to read blob from the storage container
            try
            {
                var result = await _blobStorageService.FindBlob(blobContainer, key);

                if (result == null)
                {
                    return new CacheItemMeta(Exists: false);
                }

                var stale = !result.Meta["StaleDateTime"].IsNullOrWhitespace();
                return new CacheItemMeta(Exists: true, Stale: stale);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Caught error fetching cache metadata from: {BlobContainer}/{Key}", blobContainer, key);
                return null;
            }
        }

        public void SetItem<TItem>(
            IBlobCacheKey cacheKey,
            TItem item)
        {
            throw new NotImplementedException();
        }

        public async Task SetItemAsync<TItem>(
            IBlobCacheKey cacheKey,
            TItem item)
        {
            // Write result to cache as a json blob before returning
            await _blobStorageService.UploadAsJson(cacheKey.Container, cacheKey.Key, item);

            _logger.LogDebug("Blob cache set - for key {CacheKey}", cacheKey.Key);
        }

        public async Task DeleteItemAsync(IBlobCacheKey cacheKey)
        {
            await _blobStorageService.DeleteBlob(cacheKey.Container, cacheKey.Key);

            _logger.LogDebug("Blob cache delete - for key {CacheKey}", cacheKey.Key);
        }

        public async Task DeleteCacheFolderAsync(IBlobCacheKey cacheFolderKey)
        {
            await _blobStorageService.DeleteBlobs(cacheFolderKey.Container, cacheFolderKey.Key);

            _logger.LogDebug("Blob cache folder delete - for key {CacheKey}", cacheFolderKey.Key);
        }
    }
}

record BlobCacheKeyAndType(IBlobCacheKey CacheKey, Type Type);