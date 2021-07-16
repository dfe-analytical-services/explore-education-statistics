#nullable enable
using System;
using System.IO;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;

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

        public async Task<TEntity> GetCachedEntity<TEntity>(ICacheKey<TEntity> cacheKey,
            Func<TEntity> entityProvider) where TEntity : class
        {
            // Attempt to read blob from the cache container
            var cachedEntity = await ReadFromCache(cacheKey);

            if (cachedEntity != null)
            {
                return cachedEntity;
            }

            // Cache miss - invoke provider instead
            var entity = entityProvider();

            // Write result to cache as a json blob before returning
            await WriteToCache(cacheKey, entity);
            return entity;
        }

        public async Task<TEntity> GetCachedEntity<TEntity>(ICacheKey<TEntity> cacheKey,
            Func<Task<TEntity>> entityProvider) where TEntity : class
        {
            // Attempt to read blob from the cache container
            var cachedEntity = await ReadFromCache(cacheKey);
            if (cachedEntity != null)
            {
                return cachedEntity;
            }

            // Cache miss - invoke provider instead
            var entity = await entityProvider();

            // Write result to cache as a json blob before returning
            await WriteToCache(cacheKey, entity);
            return entity;
        }

        private async Task<TEntity?> ReadFromCache<TEntity>(ICacheKey<TEntity> cacheKey) where TEntity : class
        {
            var key = cacheKey.Key;

            // Attempt to read blob from the storage container
            try
            {
                return await _blobStorageService.GetDeserializedJson<TEntity>(PublicContent, key);
            }
            catch (JsonException)
            {
                // If there's an error deserializing the blob, we should
                // assume it's not salvageable and delete it so that it's re-built.
                await _blobStorageService.DeleteBlob(PublicContent, key);
            }
            catch (FileNotFoundException)
            {
                // Do nothing as the blob just doesn't exist
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Caught error fetching cache entry from: {PublicContent}/{key}");
            }

            return null;
        }

        private async Task WriteToCache<TEntity>(ICacheKey<TEntity> cacheKey, TEntity entity)
        {
            // Write result to cache as a json blob before returning
            await _blobStorageService.UploadAsJson(PublicContent, cacheKey.Key, entity);
        }
    }
}
