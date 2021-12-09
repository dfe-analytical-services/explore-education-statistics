using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Cache.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Cache
{
    [Collection(CacheTestFixture.CollectionName)]
    public class BlobCacheAttributeTests : IClassFixture<CacheTestFixture>, IDisposable
    {
        private readonly Mock<IBlobCacheService> _blobCacheService = new(MockBehavior.Strict);

        public BlobCacheAttributeTests()
        {
            BlobCacheAttribute.AddService("default", _blobCacheService.Object);
        }

        public void Dispose()
        {
            BlobCacheAttribute.ClearServices();

            _blobCacheService.Reset();
        }

        private record TestValue
        {
            public Guid Value => Guid.NewGuid();
        }

        private record TestBlobCacheKey : IBlobCacheKey
        {
            public IBlobContainer Container => BlobContainers.PublicContent;

            public string Key { get; }

            public TestBlobCacheKey(string key)
            {
                Key = key;
            }
        }

        // ReSharper disable UnusedParameter.Local
        private static class TestMethods
        {
            [BlobCache(typeof(TestBlobCacheKey))]
            public static TestValue SingleParam(string param1)
            {
                return new();
            }

            [BlobCache(typeof(TestBlobCacheKey), ServiceName = "target")]
            public static TestValue SpecificCacheService(string param1)
            {
                return new();
            }

            [BlobCache(null!)]
            public static TestValue NullKeyType()
            {
                return new();
            }

            [BlobCache(typeof(object))]
            public static TestValue InvalidKeyType()
            {
                return new();
            }
        }
        // ReSharper enable UnusedParameter.Local

        [Fact]
        public void CacheHit()
        {
            var cacheKey = new TestBlobCacheKey("test");
            var expectedResult = new TestValue();

            _blobCacheService
                .Setup(s => s.GetItem(cacheKey, typeof(TestValue)))
                .ReturnsAsync(expectedResult);

            var result = TestMethods.SingleParam("test");

            Assert.IsType<TestValue>(result);
            Assert.Equal(expectedResult, result);

            _blobCacheService.Verify(
                s => s.GetItem(cacheKey, typeof(TestValue)), 
                Times.Once);
        }

        [Fact]
        public void CacheMiss()
        {
            var cacheKey = new TestBlobCacheKey("test");

            _blobCacheService
                .Setup(s => s.GetItem(cacheKey, typeof(TestValue)))
                .ReturnsAsync(null);

            var args = new List<object>();

            _blobCacheService
                .Setup(s => s.SetItem(cacheKey, Capture.In(args)))
                .Returns(Task.CompletedTask);

            var result = TestMethods.SingleParam("test");

            Assert.IsType<TestValue>(result);
            Assert.Equal(args[0], result);

            _blobCacheService.Verify(
                s => s.GetItem(cacheKey, typeof(TestValue)), 
                Times.Once);

            _blobCacheService.Verify(
                s => s.SetItem(cacheKey, Capture.In(args)), 
                Times.Once);
        }

        [Fact]
        public void SpecificCacheService()
        {
            var targetBlobCacheService = new Mock<IBlobCacheService>(MockBehavior.Strict);

            BlobCacheAttribute.AddService("target", targetBlobCacheService.Object);

            var cacheKey = new TestBlobCacheKey("test");

            targetBlobCacheService
                .Setup(s => s.GetItem(cacheKey, typeof(TestValue)))
                .ReturnsAsync(null);

            var args = new List<object>();

            targetBlobCacheService
                .Setup(s => s.SetItem(cacheKey, Capture.In(args)))
                .Returns(Task.CompletedTask);

            var result = TestMethods.SpecificCacheService("test");

            VerifyAllMocks(_blobCacheService, targetBlobCacheService);

            Assert.IsType<TestValue>(result);
            Assert.Equal(args[0], result);
        }

        [Fact]
        public void NoCacheService()
        {
            BlobCacheAttribute.ClearServices();

            var result = TestMethods.SingleParam("test");

            VerifyAllMocks(_blobCacheService);

            Assert.IsType<TestValue>(result);
        }

        [Fact]
        public void NullCacheKeyType()
        {
            var exception = Assert.Throws<ArgumentException>(TestMethods.NullKeyType);

            Assert.Equal($"Cache key type cannot be null", exception.Message);
        }

        [Fact]
        public void InvalidKeyType()
        {
            var exception = Assert.Throws<ArgumentException>(TestMethods.InvalidKeyType);

            Assert.Equal(
                $"Cache key type {typeof(object).FullName} must be assignable to {typeof(IBlobCacheKey).GetPrettyFullName()}",
                exception.Message
            );
        }

    }
}