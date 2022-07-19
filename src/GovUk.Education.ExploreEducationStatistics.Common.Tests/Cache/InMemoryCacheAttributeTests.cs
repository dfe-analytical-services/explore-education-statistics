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
public class InMemoryCacheAttributeTests : IClassFixture<CacheTestFixture>, IDisposable
{
    private readonly Mock<IInMemoryCacheService> _inMemoryCacheService = new(MockBehavior.Strict);

    public InMemoryCacheAttributeTests()
    {
        InMemoryCacheAttribute.AddService("default", _inMemoryCacheService.Object);
    }

    public void Dispose()
    {
        InMemoryCacheAttribute.ClearServices();

        _inMemoryCacheService.Reset();
    }

    private record TestValue;

    private record TestInMemoryCacheKey(string Key) : IInMemoryCacheKey;

    // ReSharper disable UnusedParameter.Local
    private static class TestMethods
    {
        [InMemoryCache(typeof(TestInMemoryCacheKey), expirySchedule: ExpirySchedule.Hourly, cacheDurationInSeconds: 45)]
        public static TestValue SingleParam(string param1)
        {
            return new();
        }

        [InMemoryCache(typeof(TestInMemoryCacheKey), expirySchedule: ExpirySchedule.Hourly, cacheDurationInSeconds: 45, ServiceName = "target")]
        public static TestValue SpecificCacheService(string param1)
        {
            return new();
        }

        [InMemoryCache(null!, expirySchedule: ExpirySchedule.Hourly, cacheDurationInSeconds: 45)]
        public static TestValue NullKeyType()
        {
            return new();
        }

        [InMemoryCache(typeof(object), expirySchedule: ExpirySchedule.Hourly, cacheDurationInSeconds: 45)]
        public static TestValue InvalidKeyType()
        {
            return new();
        }
            
        [InMemoryCache(typeof(TestInMemoryCacheKey), cacheDurationInSeconds: 135)]
        public static TestValue DefaultCacheConfig(string param1)
        {
            return new();
        }
    }
    // ReSharper enable UnusedParameter.Local

    [Fact]
    public void CacheHit()
    {
        var cacheKey = new TestInMemoryCacheKey("test");
        var expectedResult = new TestValue();

        _inMemoryCacheService
            .Setup(s => s.GetItem(cacheKey, typeof(TestValue)))
            .ReturnsAsync(expectedResult);

        var result = TestMethods.SingleParam("test");

        Assert.IsType<TestValue>(result);
        Assert.Equal(expectedResult, result);

        _inMemoryCacheService.Verify(
            s => s.GetItem(cacheKey, typeof(TestValue)), 
            Times.Once);
    }

    [Fact]
    public void CacheMiss()
    {
        var cacheKey = new TestInMemoryCacheKey("test");

        _inMemoryCacheService
            .Setup(s => s.GetItem(cacheKey, typeof(TestValue)))
            .ReturnsAsync(null);

        var args = new List<object>();

        var expectedCacheConfiguration = new InMemoryCacheConfiguration(ExpirySchedule.Hourly, 45);
            
        _inMemoryCacheService
            .Setup(s => s.SetItem(cacheKey, Capture.In(args), expectedCacheConfiguration, null))
            .Returns(Task.CompletedTask);

        var result = TestMethods.SingleParam("test");

        Assert.IsType<TestValue>(result);
        Assert.Equal(args[0], result);

        _inMemoryCacheService.Verify(
            s => s.GetItem(cacheKey, typeof(TestValue)), 
            Times.Once);

        _inMemoryCacheService.Verify(
            s => s.SetItem(cacheKey, Capture.In(args), expectedCacheConfiguration, null), 
            Times.Once);
    }

    [Fact]
    public void CacheMiss_DefaultCacheConfigOptions()
    {
        var cacheKey = new TestInMemoryCacheKey("test");

        _inMemoryCacheService
            .Setup(s => s.GetItem(cacheKey, typeof(TestValue)))
            .ReturnsAsync(null);

        var args = new List<object>();

        var expectedDefaultCacheConfiguration = new InMemoryCacheConfiguration(ExpirySchedule.None, 135);
            
        _inMemoryCacheService
            .Setup(s => s.SetItem(cacheKey, Capture.In(args), expectedDefaultCacheConfiguration, null))
            .Returns(Task.CompletedTask);

        var result = TestMethods.DefaultCacheConfig("test");

        Assert.IsType<TestValue>(result);
        Assert.Equal(args[0], result);

        _inMemoryCacheService.Verify(
            s => s.GetItem(cacheKey, typeof(TestValue)), 
            Times.Once);

        _inMemoryCacheService.Verify(
            s => s.SetItem(cacheKey, Capture.In(args), expectedDefaultCacheConfiguration, null), 
            Times.Once);
    }

    [Fact]
    public void SpecificCacheService()
    {
        var targetInMemoryCacheService = new Mock<IInMemoryCacheService>(MockBehavior.Strict);

        InMemoryCacheAttribute.AddService("target", targetInMemoryCacheService.Object);

        var cacheKey = new TestInMemoryCacheKey("test");

        targetInMemoryCacheService
            .Setup(s => s.GetItem(cacheKey, typeof(TestValue)))
            .ReturnsAsync(null);

        var args = new List<object>();

        var expectedCacheConfiguration = new InMemoryCacheConfiguration(ExpirySchedule.Hourly, 45);
            
        targetInMemoryCacheService
            .Setup(s => s.SetItem(cacheKey, Capture.In(args), expectedCacheConfiguration, null))
            .Returns(Task.CompletedTask);

        var result = TestMethods.SpecificCacheService("test");

        VerifyAllMocks(_inMemoryCacheService, targetInMemoryCacheService);

        Assert.IsType<TestValue>(result);
        Assert.Equal(args[0], result);
    }

    [Fact]
    public void NoCacheService()
    {
        InMemoryCacheAttribute.ClearServices();

        var result = TestMethods.SingleParam("test");

        VerifyAllMocks(_inMemoryCacheService);

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
            $"Cache key type {typeof(object).FullName} must be assignable to {typeof(IInMemoryCacheKey).GetPrettyFullName()}",
            exception.Message
        );
    }

}