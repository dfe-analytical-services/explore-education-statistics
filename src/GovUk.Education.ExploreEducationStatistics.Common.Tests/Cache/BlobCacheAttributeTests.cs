#nullable enable
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

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Cache;

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
        // ReSharper disable once UnusedMember.Local
        public Guid Value => Guid.NewGuid();
    }

    private record TestBlobCacheKey(string Key) : IBlobCacheKey
    {
        public IBlobContainer Container => BlobContainers.PublicContent;
    }

    // ReSharper disable UnusedParameter.Local
    private static class TestMethods
    {
        [BlobCache(typeof(TestBlobCacheKey))]
        public static Task<TestValue> SingleParamAsync(string _)
        {
            return Task.FromResult<TestValue>(new());
        }

        [BlobCache(typeof(TestBlobCacheKey), ServiceName = "target")]
        public static Task<TestValue> SpecificCacheServiceAsync(string _)
        {
            return Task.FromResult<TestValue>(new());
        }

        [BlobCache(null!)]
        public static Task<TestValue> NullKeyTypeAsync()
        {
            return Task.FromResult<TestValue>(new());
        }

        [BlobCache(typeof(object))]
        public static Task<TestValue> InvalidKeyTypeAsync()
        {
            return Task.FromResult<TestValue>(new());
        }

        [BlobCache(typeof(TestBlobCacheKey))]
        public static TestValue SingleParam(string _)
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
            .Returns(expectedResult);

        var result = TestMethods.SingleParam("test");

        Assert.IsType<TestValue>(result);
        Assert.Equal(expectedResult, result);

        _blobCacheService.Verify(
            s => s.GetItem(cacheKey, typeof(TestValue)),
            Times.Once);
    }

    [Fact]
    public async Task CacheHitAsync()
    {
        var cacheKey = new TestBlobCacheKey("test");
        var expectedResult = new TestValue();

        _blobCacheService
            .Setup(s => s.GetItemAsync(cacheKey, typeof(TestValue)))
            .ReturnsAsync(expectedResult);

        var result = await TestMethods.SingleParamAsync("test");

        Assert.IsType<TestValue>(result);
        Assert.Equal(expectedResult, result);

        _blobCacheService.Verify(
            s => s.GetItemAsync(cacheKey, typeof(TestValue)), 
            Times.Once);
    }

    [Fact]
    public void CacheMiss()
    {
        var cacheKey = new TestBlobCacheKey("test");

        _blobCacheService
            .Setup(s => s.GetItem(cacheKey, typeof(TestValue)))
            .Returns((object?)null);

        var args = new List<object>();

        _blobCacheService
            .Setup(s => s.SetItem(cacheKey, Capture.In(args)));

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
    public async Task CacheMissAsync()
    {
        var cacheKey = new TestBlobCacheKey("test");

        _blobCacheService
            .Setup(s => s.GetItemAsync(cacheKey, typeof(TestValue)))
            .ReturnsAsync((object?)null);

        var args = new List<object>();

        _blobCacheService
            .Setup(s => s.SetItemAsync(cacheKey, Capture.In(args)))
            .Returns(Task.CompletedTask);

        var result = await TestMethods.SingleParamAsync("test");

        Assert.IsType<TestValue>(result);
        Assert.Equal(args[0], result);

        _blobCacheService.Verify(
            s => s.GetItemAsync(cacheKey, typeof(TestValue)), 
            Times.Once);

        _blobCacheService.Verify(
            s => s.SetItemAsync(cacheKey, Capture.In(args)), 
            Times.Once);
    }

    [Fact]
    public async Task SpecificCacheService()
    {
        var targetBlobCacheService = new Mock<IBlobCacheService>(MockBehavior.Strict);

        BlobCacheAttribute.AddService("target", targetBlobCacheService.Object);

        var cacheKey = new TestBlobCacheKey("test");

        targetBlobCacheService
            .Setup(s => s.GetItemAsync(cacheKey, typeof(TestValue)))
            .ReturnsAsync((object?)null);

        var args = new List<object>();

        targetBlobCacheService
            .Setup(s => s.SetItemAsync(cacheKey, Capture.In(args)))
            .Returns(Task.CompletedTask);

        var result = await TestMethods.SpecificCacheServiceAsync("test");

        VerifyAllMocks(_blobCacheService, targetBlobCacheService);

        Assert.IsType<TestValue>(result);
        Assert.Equal(args[0], result);
    }

    [Fact]
    public async Task NoCacheService()
    {
        BlobCacheAttribute.ClearServices();

        var result = await TestMethods.SingleParamAsync("test");

        VerifyAllMocks(_blobCacheService);

        Assert.IsType<TestValue>(result);
    }

    [Fact]
    public async Task NullCacheKeyType()
    {
        var exception = await Assert.ThrowsAsync<ArgumentException>(TestMethods.NullKeyTypeAsync);

        Assert.Equal($"Cache key type cannot be null", exception.Message);
    }

    [Fact]
    public async Task InvalidKeyType()
    {
        var exception = await Assert.ThrowsAsync<ArgumentException>(TestMethods.InvalidKeyTypeAsync);

        Assert.Equal(
            $"Cache key type {typeof(object).FullName} must be assignable to {typeof(IBlobCacheKey).GetPrettyFullName()}",
            exception.Message
        );
    }
}
