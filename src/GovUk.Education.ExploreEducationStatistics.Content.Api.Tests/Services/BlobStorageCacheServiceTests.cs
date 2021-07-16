#nullable enable
using System;
using System.IO;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Services
{
    public class BlobStorageCacheServiceTests
    {
        private class SampleClass
        {
            public Guid Id { get; }

            public SampleClass()
            {
                Id = Guid.NewGuid();
            }
        }

        private class SampleCacheKey : ICacheKey<SampleClass>
        {
            public string Key { get; }

            public SampleCacheKey(Guid id)
            {
                Key = id.ToString();
            }
        }

        [Fact]
        public async Task GetCachedEntity_CachedEntityExists()
        {
            var entity = new SampleClass();

            var cacheKey = new SampleCacheKey(entity.Id);

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            blobStorageService.Setup(mock =>
                    mock.GetDeserializedJson<SampleClass>(
                        PublicContent,
                        cacheKey.Key))
                .ReturnsAsync(entity);

            var service = SetupBlobStorageCacheService(blobStorageService: blobStorageService.Object);

            var result = await service.GetCachedEntity(cacheKey, (Func<SampleClass>) (() =>
                throw new ArgumentException("Unexpected call to provider when cached entity exists")));

            Assert.Equal(entity, result);

            MockUtils.VerifyAllMocks(blobStorageService);
        }

        [Fact]
        public async Task GetCachedEntity_CachedEntityNotFoundUploadsBlob()
        {
            var entity = new SampleClass();

            var cacheKey = new SampleCacheKey(entity.Id);

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            blobStorageService.Setup(mock =>
                    mock.GetDeserializedJson<SampleClass>(
                        PublicContent,
                        cacheKey.Key))
                .ThrowsAsync(new FileNotFoundException());

            blobStorageService.Setup(mock =>
                    mock.UploadAsJson(
                        PublicContent,
                        cacheKey.Key,
                        entity,
                        null))
                .Returns(Task.CompletedTask);

            var service = SetupBlobStorageCacheService(blobStorageService: blobStorageService.Object);

            var result = await service.GetCachedEntity(cacheKey, () => entity);

            Assert.Equal(entity, result);

            MockUtils.VerifyAllMocks(blobStorageService);
        }

        [Fact]
        public async Task GetCachedEntity_ExceptionFetchingCachedEntityUploadsBlob()
        {
            var entity = new SampleClass();

            var cacheKey = new SampleCacheKey(entity.Id);

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            blobStorageService.Setup(mock =>
                    mock.GetDeserializedJson<SampleClass>(
                        PublicContent,
                        cacheKey.Key))
                .ThrowsAsync(new Exception("An error occurred"));

            blobStorageService.Setup(mock =>
                    mock.UploadAsJson(
                        PublicContent,
                        cacheKey.Key,
                        entity,
                        null))
                .Returns(Task.CompletedTask);

            var service = SetupBlobStorageCacheService(blobStorageService: blobStorageService.Object);

            var result = await service.GetCachedEntity(cacheKey, () => entity);

            Assert.Equal(entity, result);

            MockUtils.VerifyAllMocks(blobStorageService);
        }

        [Fact]
        public async Task GetCachedEntity_ErrorDeserializingCachedEntityDeletesAndUploadsBlob()
        {
            var entity = new SampleClass();

            var cacheKey = new SampleCacheKey(entity.Id);

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            blobStorageService.Setup(mock =>
                    mock.GetDeserializedJson<SampleClass>(
                        PublicContent,
                        cacheKey.Key))
                .ThrowsAsync(new JsonException("Could not deserialize the JSON"));

            blobStorageService.Setup(mock =>
                    mock.DeleteBlob(PublicContent,
                        cacheKey.Key))
                .Returns(Task.CompletedTask);

            blobStorageService.Setup(mock =>
                    mock.UploadAsJson(
                        PublicContent,
                        cacheKey.Key,
                        entity,
                        null))
                .Returns(Task.CompletedTask);

            var service = SetupBlobStorageCacheService(blobStorageService: blobStorageService.Object);

            var result = await service.GetCachedEntity(cacheKey, () => entity);

            Assert.Equal(entity, result);

            MockUtils.VerifyAllMocks(blobStorageService);
        }

        private static BlobStorageCacheService SetupBlobStorageCacheService(
            IBlobStorageService? blobStorageService = null,
            ILogger<BlobStorageCacheService>? logger = null)
        {
            return new BlobStorageCacheService(
                blobStorageService ?? new Mock<IBlobStorageService>().Object,
                logger ?? new Mock<ILogger<BlobStorageCacheService>>().Object
            );
        }
    }
}
