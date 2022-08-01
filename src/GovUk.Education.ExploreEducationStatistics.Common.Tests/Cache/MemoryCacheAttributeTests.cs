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
public class MemoryCacheAttributeTests : IClassFixture<CacheTestFixture>, IDisposable
{
    private readonly Mock<IMemoryCacheService> _memoryCacheService = new(MockBehavior.Strict);

    public MemoryCacheAttributeTests()
    {
        MemoryCacheAttribute.AddService("default", _memoryCacheService.Object);
    }

    public void Dispose()
    {
        MemoryCacheAttribute.ClearServices();

        _memoryCacheService.Reset();
    }

    private record TestValue;

    private record TestMemoryCacheKey(string Key) : IMemoryCacheKey;

    // ReSharper disable UnusedParameter.Local
    private static class TestMethods
    {
        [MemoryCache(typeof(TestMemoryCacheKey), expirySchedule: ExpirySchedule.Hourly, cacheDurationInSeconds: 45)]
        public static TestValue SingleParam(string param1)
        {
            return new();
        }

        [MemoryCache(typeof(TestMemoryCacheKey), expirySchedule: ExpirySchedule.Hourly, cacheDurationInSeconds: 45, ServiceName = "target")]
        public static TestValue SpecificCacheService(string param1)
        {
            return new();
        }

        [MemoryCache(null!, expirySchedule: ExpirySchedule.Hourly, cacheDurationInSeconds: 45)]
        public static TestValue NullKeyType()
        {
            return new();
        }

        [MemoryCache(typeof(object), expirySchedule: ExpirySchedule.Hourly, cacheDurationInSeconds: 45)]
        public static TestValue InvalidKeyType()
        {
            return new();
        }
            
        [MemoryCache(typeof(TestMemoryCacheKey), cacheDurationInSeconds: 135)]
        public static TestValue DefaultCacheConfig(string param1)
        {
            return new();
        }
    }
    // ReSharper enable UnusedParameter.Local

    [Fact]
    public void CacheHit()
    {
        var cacheKey = new TestMemoryCacheKey("test");
        var expectedResult = new TestValue();

        _memoryCacheService
            .Setup(s => s.GetItem(cacheKey, typeof(TestValue)))
            .ReturnsAsync(expectedResult);

        var result = TestMethods.SingleParam("test");

        Assert.IsType<TestValue>(result);
        Assert.Equal(expectedResult, result);

        _memoryCacheService.Verify(
            s => s.GetItem(cacheKey, typeof(TestValue)), 
            Times.Once);
    }

    [Fact]
    public void CacheMiss()
    {
        var cacheKey = new TestMemoryCacheKey("test");

        _memoryCacheService
            .Setup(s => s.GetItem(cacheKey, typeof(TestValue)))
            .ReturnsAsync(null);

        var args = new List<object>();

        var expectedCacheConfiguration = new MemoryCacheConfiguration(ExpirySchedule.Hourly, 45);
            
        _memoryCacheService
            .Setup(s => s.SetItem(cacheKey, Capture.In(args), expectedCacheConfiguration, null))
            .Returns(Task.CompletedTask);

        var result = TestMethods.SingleParam("test");

        Assert.IsType<TestValue>(result);
        Assert.Equal(args[0], result);

        _memoryCacheService.Verify(
            s => s.GetItem(cacheKey, typeof(TestValue)), 
            Times.Once);

        _memoryCacheService.Verify(
            s => s.SetItem(cacheKey, Capture.In(args), expectedCacheConfiguration, null), 
            Times.Once);
    }

    [Fact]
    public void CacheMiss_DefaultCacheConfigOptions()
    {
        var cacheKey = new TestMemoryCacheKey("test");

        _memoryCacheService
            .Setup(s => s.GetItem(cacheKey, typeof(TestValue)))
            .ReturnsAsync(null);

        var args = new List<object>();

        var expectedDefaultCacheConfiguration = new MemoryCacheConfiguration(ExpirySchedule.None, 135);
            
        _memoryCacheService
            .Setup(s => s.SetItem(cacheKey, Capture.In(args), expectedDefaultCacheConfiguration, null))
            .Returns(Task.CompletedTask);

        var result = TestMethods.DefaultCacheConfig("test");

        Assert.IsType<TestValue>(result);
        Assert.Equal(args[0], result);

        _memoryCacheService.Verify(
            s => s.GetItem(cacheKey, typeof(TestValue)), 
            Times.Once);

        _memoryCacheService.Verify(
            s => s.SetItem(cacheKey, Capture.In(args), expectedDefaultCacheConfiguration, null), 
            Times.Once);
    }

    [Fact]
    public void SpecificCacheService()
    {
        var targetMemoryCacheService = new Mock<IMemoryCacheService>(MockBehavior.Strict);

        MemoryCacheAttribute.AddService("target", targetMemoryCacheService.Object);

        var cacheKey = new TestMemoryCacheKey("test");

        targetMemoryCacheService
            .Setup(s => s.GetItem(cacheKey, typeof(TestValue)))
            .ReturnsAsync(null);

        var args = new List<object>();

        var expectedCacheConfiguration = new MemoryCacheConfiguration(ExpirySchedule.Hourly, 45);
            
        targetMemoryCacheService
            .Setup(s => s.SetItem(cacheKey, Capture.In(args), expectedCacheConfiguration, null))
            .Returns(Task.CompletedTask);

        var result = TestMethods.SpecificCacheService("test");

        VerifyAllMocks(_memoryCacheService, targetMemoryCacheService);

        Assert.IsType<TestValue>(result);
        Assert.Equal(args[0], result);
    }

    [Fact]
    public void NoCacheService()
    {
        MemoryCacheAttribute.ClearServices();

        var result = TestMethods.SingleParam("test");

        VerifyAllMocks(_memoryCacheService);

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
            $"Cache key type {typeof(object).FullName} must be assignable to {typeof(IMemoryCacheKey).GetPrettyFullName()}",
            exception.Message
        );
    }

}