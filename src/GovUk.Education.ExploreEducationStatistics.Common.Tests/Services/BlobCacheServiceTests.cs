#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Cache.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Services
{
    public class BlobCacheServiceTests
    {
        private record SampleClass(Guid Id);

        private class SampleCacheKey : IBlobCacheKey
        {
            public IBlobContainer Container { get; }
            public string Key { get; }

            public SampleCacheKey(IBlobContainer container, Guid id)
            {
                Container = container;
                Key = id.ToString();
            }
        }

        [Fact]
        public void SetItem()
        {
            var entity = new SampleClass(Guid.NewGuid());

            var cacheKey = new SampleCacheKey(PublicContent, entity.Id);

            var service = SetupBlobStorageCacheService();

            Assert.Throws<NotImplementedException>(() => service.SetItem(cacheKey, typeof(SampleClass)));
        }

        [Fact]
        public async Task SetItemAsync()
        {
            var cacheKey = new SampleCacheKey(PublicContent, Guid.NewGuid());

            var blobStorageService = new Mock<IPublicBlobStorageService>(MockBehavior.Strict);

            blobStorageService.SetupUploadAsJson(
                container: PublicContent,
                path: cacheKey.Key,
                content: "test item");

            var service = SetupBlobStorageCacheService(blobStorageService: blobStorageService.Object);

            await service.SetItemAsync(cacheKey, "test item");

            VerifyAllMocks(blobStorageService);
        }

        [Fact]
        public async Task DeleteItemAsync()
        {
            var cacheKey = new SampleCacheKey(PublicContent, Guid.NewGuid());

            var blobStorageService = new Mock<IPublicBlobStorageService>(MockBehavior.Strict);

            blobStorageService.SetupDeleteBlob(PublicContent, cacheKey.Key);

            var service = SetupBlobStorageCacheService(blobStorageService: blobStorageService.Object);

            await service.DeleteItemAsync(cacheKey);

            VerifyAllMocks(blobStorageService);
        }

        [Fact]
        public void GetItem()
        {
            var entity = new SampleClass(Guid.NewGuid());

            var cacheKey = new SampleCacheKey(PublicContent, entity.Id);

            var service = SetupBlobStorageCacheService();

            Assert.Throws<NotImplementedException>(() => service.GetItem(cacheKey, typeof(SampleClass)));
        }

        [Fact]
        public async Task GetItemAsync()
        {
            var entity = new SampleClass(Guid.NewGuid());

            var cacheKey = new SampleCacheKey(PublicContent, entity.Id);

            var blobStorageService = new Mock<IPublicBlobStorageService>(MockBehavior.Strict);

            blobStorageService.SetupGetDeserializedJson(
                container: PublicContent,
                path: cacheKey.Key,
                value: entity,
                type: entity.GetType());

            var service = SetupBlobStorageCacheService(blobStorageService: blobStorageService.Object);

            var result = await service.GetItemAsync(cacheKey, typeof(SampleClass));
            var typedResult = Assert.IsType<SampleClass>(result);

            Assert.Equal(entity, typedResult);

            VerifyAllMocks(blobStorageService);
        }

        [Fact]
        public async Task GetItemAsync_NullIfErrorDeserializingCachedEntity()
        {
            var cacheKey = new SampleCacheKey(PublicContent, Guid.NewGuid());

            var blobStorageService = new Mock<IPublicBlobStorageService>(MockBehavior.Strict);

            blobStorageService.SetupGetDeserializedJsonThrows(
                container: PublicContent,
                path: cacheKey.Key,
                type: typeof(SampleClass),
                exception: new JsonException("Could not deserialize"));

            blobStorageService.SetupDeleteBlob(PublicContent, cacheKey.Key);

            var service = SetupBlobStorageCacheService(blobStorageService: blobStorageService.Object);

            var result = await service.GetItemAsync(cacheKey, typeof(SampleClass));

            Assert.Null(result);

            VerifyAllMocks(blobStorageService);
        }

        [Fact]
        public async Task GetItemAsync_NullIfNotFound()
        {
            var cacheKey = new SampleCacheKey(PublicContent, Guid.NewGuid());

            var blobStorageService = new Mock<IPublicBlobStorageService>(MockBehavior.Strict);

            blobStorageService.SetupGetDeserializedJsonNotFound(
                container: PublicContent,
                path: cacheKey.Key,
                type: typeof(SampleClass));

            var service = SetupBlobStorageCacheService(blobStorageService: blobStorageService.Object);

            var result = await service.GetItemAsync(cacheKey, typeof(SampleClass));

            Assert.Null(result);

            VerifyAllMocks(blobStorageService);
        }

        [Fact]
        public async Task GetItemAsync_NullIfException()
        {
            var cacheKey = new SampleCacheKey(PublicContent, Guid.NewGuid());

            var blobStorageService = new Mock<IPublicBlobStorageService>(MockBehavior.Strict);

            blobStorageService.SetupGetDeserializedJsonThrows(
                container: PublicContent,
                path: cacheKey.Key,
                type: typeof(SampleClass),
                exception: new Exception("Something went wrong"));

            var service = SetupBlobStorageCacheService(blobStorageService: blobStorageService.Object);

            var result = await service.GetItemAsync(cacheKey, typeof(SampleClass));

            Assert.Null(result);

            VerifyAllMocks(blobStorageService);
        }

        private static BlobCacheService SetupBlobStorageCacheService(
            IBlobStorageService? blobStorageService = null,
            ILogger<BlobCacheService>? logger = null)
        {
            return new (
                blobStorageService ?? Mock.Of<IBlobStorageService>(),
                logger ?? Mock.Of<ILogger<BlobCacheService>>()
            );
        }
    }
}
