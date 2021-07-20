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
    public class BlobStorageCacheService : ICacheService
    {
        private readonly IBlobStorageService _blobStorageService;
        private readonly ILogger<BlobStorageCacheService> _logger;

        public BlobStorageCacheService(IBlobStorageService blobStorageService,
            ILogger<BlobStorageCacheService> logger)
        {
            _blobStorageService = blobStorageService;
            _logger = logger;
        }

        public async Task DeleteItem(IBlobContainer blobContainer, ICacheKey cacheKey)
        {
            await _blobStorageService.DeleteBlob(blobContainer, cacheKey.Key);
        }

        public async Task<TEntity> GetItem<TEntity>(
            IBlobContainer blobContainer,
            ICacheKey cacheKey,
            Func<TEntity> entityProvider) where TEntity : class
        {
            // Attempt to read blob from the cache container
            var cachedEntity = await ReadFromCache<TEntity>(blobContainer, cacheKey);

            if (cachedEntity != null)
            {
                return cachedEntity;
            }

            // Cache miss - invoke provider instead
            var entity = entityProvider();

            // Write result to cache as a json blob before returning
            await WriteToCache(blobContainer, cacheKey, entity);
            return entity;
        }

        public async Task<TEntity> GetItem<TEntity>(
            IBlobContainer blobContainer,
            ICacheKey cacheKey,
            Func<Task<TEntity>> entityProvider)
            where TEntity : class
        {
            // Attempt to read blob from the cache container
            var cachedEntity = await ReadFromCache<TEntity>(blobContainer, cacheKey);
            if (cachedEntity != null)
            {
                return cachedEntity;
            }

            // Cache miss - invoke provider instead
            var entity = await entityProvider();

            // Write result to cache as a json blob before returning
            await WriteToCache(blobContainer, cacheKey, entity);
            return entity;
        }

        public async Task<Either<ActionResult, TEntity>> GetItem<TEntity>(
            IBlobContainer blobContainer,
            ICacheKey cacheKey,
            Func<Task<Either<ActionResult, TEntity>>> entityProvider)
            where TEntity : class
        {
            // Attempt to read blob from the cache container
            var cachedEntity = await ReadFromCache<TEntity>(blobContainer, cacheKey);
            if (cachedEntity != null)
            {
                return cachedEntity;
            }

            // Cache miss - invoke provider instead
            return await entityProvider().OnSuccessDo(async entity =>
            {
                // Write result to cache as a json blob before returning
                await WriteToCache(blobContainer, cacheKey, entity);
            });
        }

        private async Task<TEntity?> ReadFromCache<TEntity>(
            IBlobContainer blobContainer,
            ICacheKey cacheKey)
            where TEntity : class
        {
            var key = cacheKey.Key;

            // Attempt to read blob from the storage container
            try
            {
                return await _blobStorageService.GetDeserializedJson<TEntity>(blobContainer, key);
            }
            catch (JsonException)
            {
                // If there's an error deserializing the blob, we should
                // assume it's not salvageable and delete it so that it's re-built.
                await _blobStorageService.DeleteBlob(blobContainer, key);
            }
            catch (FileNotFoundException)
            {
                // Do nothing as the blob just doesn't exist
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Caught error fetching cache entry from: {blobContainer}/{key}");
            }

            return null;
        }

        private async Task WriteToCache<TEntity>(
            IBlobContainer blobContainer,
            ICacheKey cacheKey,
            TEntity entity)
        {
            // Write result to cache as a json blob before returning
            await _blobStorageService.UploadAsJson(blobContainer, cacheKey.Key, entity);
        }
    }
}
