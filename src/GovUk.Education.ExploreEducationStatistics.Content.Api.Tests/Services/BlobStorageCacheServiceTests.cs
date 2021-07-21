#nullable enable
using System;
using System.IO;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Cache.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;

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

        private class SampleCacheKey : ICacheKey
        {
            public string Key { get; }

            public SampleCacheKey(Guid id)
            {
                Key = id.ToString();
            }
        }

        [Fact]
        public async Task DeleteItem()
        {
            var cacheKey = new SampleCacheKey(Guid.NewGuid());

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            blobStorageService.Setup(mock =>
                    mock.DeleteBlob(PublicContent,
                        cacheKey.Key))
                .Returns(Task.CompletedTask);

            var service = SetupBlobStorageCacheService(blobStorageService: blobStorageService.Object);

            await service.DeleteItem(PublicContent, cacheKey);

            VerifyAllMocks(blobStorageService);
        }

        [Fact]
        public async Task GetItem_CachedEntityExists()
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

            var result = await service.GetItem(PublicContent,
                cacheKey,
                (Func<SampleClass>) (() =>
                    throw new ArgumentException("Unexpected call to provider when cached entity exists")));

            Assert.Equal(entity, result);

            VerifyAllMocks(blobStorageService);
        }

        [Fact]
        public async Task GetItem_CachedEntityNotFoundUploadsBlob()
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

            var result = await service.GetItem(PublicContent,
                cacheKey,
                () => entity);

            Assert.Equal(entity, result);

            VerifyAllMocks(blobStorageService);
        }

        [Fact]
        public async Task GetItem_ProviderReturnsRightUploadsBlob()
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

            var result = (await service.GetItem(PublicContent,
                cacheKey,
                () => Task.FromResult(new Either<ActionResult, SampleClass>(entity)))).AssertRight();

            Assert.Equal(entity, result);

            VerifyAllMocks(blobStorageService);
        }

        [Fact]
        public async Task GetItem_ProviderReturnsLeft()
        {
            var entity = new SampleClass();

            var cacheKey = new SampleCacheKey(entity.Id);

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            blobStorageService.Setup(mock =>
                    mock.GetDeserializedJson<SampleClass>(
                        PublicContent,
                        cacheKey.Key))
                .ThrowsAsync(new FileNotFoundException());

            var service = SetupBlobStorageCacheService(blobStorageService: blobStorageService.Object);

            var result = await service.GetItem(PublicContent,
                cacheKey,
                () => Task.FromResult(new Either<ActionResult, SampleClass>(new NotFoundResult())));

            result.AssertNotFound();

            VerifyAllMocks(blobStorageService);
        }

        [Fact]
        public async Task GetItem_ExceptionFetchingCachedEntityUploadsBlob()
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

            var result = await service.GetItem(
                PublicContent,
                cacheKey,
                () => entity);

            Assert.Equal(entity, result);

            VerifyAllMocks(blobStorageService);
        }

        [Fact]
        public async Task GetItem_ErrorDeserializingCachedEntityDeletesAndUploadsBlob()
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

            var result = await service.GetItem(
                PublicContent,
                cacheKey,
                () => entity);

            Assert.Equal(entity, result);

            VerifyAllMocks(blobStorageService);
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
