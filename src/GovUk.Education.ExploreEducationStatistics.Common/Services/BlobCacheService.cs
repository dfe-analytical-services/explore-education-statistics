#nullable enable
using System;
using System.IO;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Cache.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services
{
    public class BlobCacheService : IBlobCacheService
    {
        private readonly IBlobStorageService _blobStorageService;
        private readonly ILogger<BlobCacheService> _logger;

        public BlobCacheService(IBlobStorageService blobStorageService,
            ILogger<BlobCacheService> logger)
        {
            _blobStorageService = blobStorageService;
            _logger = logger;
        }

        public async Task DeleteItem(IBlobCacheKey cacheKey)
        {
            await _blobStorageService.DeleteBlob(cacheKey.Container, cacheKey.Key);
        }

        public async Task DeleteCacheFolder(IBlobCacheKey cacheFolderKey)
        {
            await _blobStorageService.DeleteBlobs(cacheFolderKey.Container, cacheFolderKey.Key);
        }

        public async Task<TItem> GetItem<TItem>(
            IBlobCacheKey cacheKey,
            Func<TItem> itemSupplier)
            where TItem : class
        {
            // Attempt to read blob from the cache container
            var cachedEntity = await GetItem<TItem>(cacheKey);

            if (cachedEntity != null)
            {
                return cachedEntity;
            }

            // Cache miss - invoke provider instead
            var entity = itemSupplier();

            // Write result to cache as a json blob before returning
            await SetItem(cacheKey, entity);
            return entity;
        }

        public async Task<TItem> GetItem<TItem>(
            IBlobCacheKey cacheKey,
            Func<Task<TItem>> itemSupplier)
            where TItem : class
        {
            // Attempt to read blob from the cache container
            var cachedEntity = await GetItem<TItem>(cacheKey);
            if (cachedEntity != null)
            {
                return cachedEntity;
            }

            // Cache miss - invoke provider instead
            var entity = await itemSupplier();

            // Write result to cache as a json blob before returning
            await SetItem(cacheKey, entity);
            return entity;
        }

        public async Task<Either<ActionResult, TItem>> GetItem<TItem>(
            IBlobCacheKey cacheKey,
            Func<Task<Either<ActionResult, TItem>>> itemSupplier)
            where TItem : class
        {
            // Attempt to read blob from the cache container
            var cachedEntity = await GetItem<TItem>(cacheKey);

            if (cachedEntity != null)
            {
                return cachedEntity;
            }

            // Cache miss - invoke provider instead
            return await itemSupplier().OnSuccessDo(async entity =>
            {
                // Write result to cache as a json blob before returning
                await SetItem(cacheKey, entity);
            });
        }

        public async Task<TItem?> GetItem<TItem>(IBlobCacheKey cacheKey)
            where TItem : class
        {
            return (TItem?) await GetItem(cacheKey, typeof(TItem));
        }

        public async Task<object?> GetItem(IBlobCacheKey cacheKey, Type targetType)
        {
            var blobContainer = cacheKey.Container;
            var key = cacheKey.Key;

            // Attempt to read blob from the storage container
            try
            {
                return await _blobStorageService.GetDeserializedJson(cacheKey.Container, cacheKey.Key, targetType);
            }
            catch (JsonException e)
            {
                // If there's an error deserializing the blob, we should
                // assume it's not salvageable and delete it so that it's re-built.
                _logger.LogWarning(e, $"Error deserializing JSON for blobContainer {blobContainer} and cache " +
                                   $"key {key} - deleting cached JSON");
                await _blobStorageService.DeleteBlob(blobContainer, key);
            }
            catch (FileNotFoundException)
            {
                // Do nothing as the blob just doesn't exist
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Caught error fetching cache entry from: {BlobContainer}/{Key}", blobContainer, key);
            }

            return default;
        }

        public async Task SetItem<TItem>(
            IBlobCacheKey cacheKey,
            TItem item)
        {
            // Write result to cache as a json blob before returning
            await _blobStorageService.UploadAsJson(cacheKey.Container, cacheKey.Key, item);
        }
    }
}
