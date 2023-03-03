#nullable enable
using System;
using System.IO;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Cache.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
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
        private record SampleClass
        {
            public Guid Id { get; }

            public SampleClass()
            {
                Id = Guid.NewGuid();
            }
        }

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
        public async Task SetItem()
        {
            var cacheKey = new SampleCacheKey(PublicContent, Guid.NewGuid());

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            blobStorageService.Setup(mock =>
                    mock.UploadAsJson(PublicContent, cacheKey.Key, "test item", null))
                .Returns(Task.CompletedTask);

            var service = SetupBlobStorageCacheService(blobStorageService: blobStorageService.Object);

            await service.SetItem(cacheKey, "test item");

            VerifyAllMocks(blobStorageService);
        }

        [Fact]
        public async Task DeleteItem()
        {
            var cacheKey = new SampleCacheKey(PublicContent, Guid.NewGuid());

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            blobStorageService.Setup(mock =>
                    mock.DeleteBlob(PublicContent, cacheKey.Key))
                .Returns(Task.CompletedTask);

            var service = SetupBlobStorageCacheService(blobStorageService: blobStorageService.Object);

            await service.DeleteItem(cacheKey);

            VerifyAllMocks(blobStorageService);
        }

        [Fact]
        public async Task GetItem()
        {
            var entity = new SampleClass();

            var cacheKey = new SampleCacheKey(PublicContent, entity.Id);

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            blobStorageService.Setup(mock =>
                    mock.GetDeserializedJson(
                        PublicContent, cacheKey.Key, typeof(SampleClass), default))
                .ReturnsAsync(entity);

            var service = SetupBlobStorageCacheService(blobStorageService: blobStorageService.Object);

            var result = await service.GetItem(cacheKey, typeof(SampleClass));
            var typedResult = Assert.IsType<SampleClass>(result);

            Assert.Equal(entity, typedResult);

            VerifyAllMocks(blobStorageService);
        }

        [Fact]
        public async Task GetItem_NullIfErrorDeserializingCachedEntity()
        {
            var cacheKey = new SampleCacheKey(PublicContent, Guid.NewGuid());

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            blobStorageService.Setup(mock =>
                    mock.GetDeserializedJson(
                        PublicContent, cacheKey.Key, typeof(SampleClass), default))
                .ThrowsAsync(new JsonException("Could not deserialize"));


            blobStorageService.Setup(mock =>
                    mock.DeleteBlob(PublicContent, cacheKey.Key))
                .Returns(Task.CompletedTask);

            var service = SetupBlobStorageCacheService(blobStorageService: blobStorageService.Object);

            var result = await service.GetItem(cacheKey, typeof(SampleClass));

            Assert.Null(result);

            VerifyAllMocks(blobStorageService);
        }

        [Fact]
        public async Task GetItem_NullIfNoFileFoundException()
        {
            var cacheKey = new SampleCacheKey(PublicContent, Guid.NewGuid());

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            blobStorageService.Setup(mock =>
                    mock.GetDeserializedJson(
                        PublicContent, cacheKey.Key, typeof(SampleClass), default))
                .ThrowsAsync(new FileNotFoundException());

            var service = SetupBlobStorageCacheService(blobStorageService: blobStorageService.Object);

            var result = await service.GetItem(cacheKey, typeof(SampleClass));

            Assert.Null(result);

            VerifyAllMocks(blobStorageService);
        }

        [Fact]
        public async Task GetItem_NullIfException()
        {
            var cacheKey = new SampleCacheKey(PublicContent, Guid.NewGuid());

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            blobStorageService.Setup(mock =>
                    mock.GetDeserializedJson(
                        PublicContent, cacheKey.Key, typeof(SampleClass), default))
                .ThrowsAsync(new Exception("Something went wrong"));

            var service = SetupBlobStorageCacheService(blobStorageService: blobStorageService.Object);

            var result = await service.GetItem(cacheKey, typeof(SampleClass));

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
