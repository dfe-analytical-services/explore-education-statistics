using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cronos;
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Cache.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Cache;

[Collection(CacheTestFixture.CollectionName)]
public class MemoryCacheAttributeTests : IClassFixture<CacheTestFixture>, IDisposable
{
    private const string HourlyExpirySchedule = "0 * * * *";
    private const string HalfHourlyExpirySchedule = "*/30 * * * *";
    
    private readonly Mock<IMemoryCacheService> _memoryCacheService = new(MockBehavior.Strict);

    public MemoryCacheAttributeTests()
    {
        MemoryCacheAttribute.AddService("default", _memoryCacheService.Object);
    }

    public void Dispose()
    {
        MemoryCacheAttribute.ClearServices();
        MemoryCacheAttribute.SetConfiguration(null);

        _memoryCacheService.Reset();
    }

    private record TestValue;

    private record TestMemoryCacheKey(string Key) : IMemoryCacheKey;

    // ReSharper disable UnusedParameter.Local
    private static class TestMethods
    {
        [MemoryCache(typeof(TestMemoryCacheKey), expiryScheduleCron: HourlyExpirySchedule, cacheDurationInSeconds: 45)]
        public static TestValue SingleParam(string param1)
        {
            return new();
        }

        [MemoryCache(typeof(TestMemoryCacheKey), expiryScheduleCron: HourlyExpirySchedule, cacheDurationInSeconds: 45, ServiceName = "target")]
        public static TestValue SpecificCacheService(string param1)
        {
            return new();
        }

        [MemoryCache(null!, expiryScheduleCron: HourlyExpirySchedule, cacheDurationInSeconds: 45)]
        public static TestValue NullKeyType()
        {
            return new();
        }

        [MemoryCache(typeof(object), expiryScheduleCron: HourlyExpirySchedule, cacheDurationInSeconds: 45)]
        public static TestValue InvalidKeyType()
        {
            return new();
        }

        [MemoryCache(typeof(TestMemoryCacheKey), cacheConfigKey: "SpecificCacheConfigurationKey")]
        public static TestValue SpecificCacheConfigurationKey(string param1)
        {
            return new();
        }

        [MemoryCache(typeof(TestMemoryCacheKey), cacheConfigKey: "UnspecifiedCacheConfigurationKey")]
        public static TestValue UnspecifiedCacheConfigurationKey(string param1)
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

        var expectedCacheConfiguration = new MemoryCacheConfiguration(45, CronExpression.Parse(HourlyExpirySchedule));
            
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

        var expectedDefaultCacheConfiguration = new MemoryCacheConfiguration(135);
            
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

        var expectedCacheConfiguration = new MemoryCacheConfiguration(45, CronExpression.Parse(HourlyExpirySchedule));
            
        targetMemoryCacheService
            .Setup(s => s.SetItem(cacheKey, Capture.In(args), expectedCacheConfiguration, null))
            .Returns(Task.CompletedTask);

        var result = TestMethods.SpecificCacheService("test");

        VerifyAllMocks(_memoryCacheService, targetMemoryCacheService);

        Assert.IsType<TestValue>(result);
        Assert.Equal(args[0], result);
    }

    [Fact]
    public void SpecificCacheConfigurationKey()
    {
        var configuration = new Mock<IConfigurationSection>(MockBehavior.Strict);
        var specificCacheConfiguration = CreateMockConfigurationSection(
            TupleOf("CacheDurationInSeconds", "35"),
            TupleOf("ExpirySchedule", HalfHourlyExpirySchedule));
        configuration
            .Setup(s => s.GetSection("SpecificCacheConfigurationKey"))
            .Returns(specificCacheConfiguration.Object);
        MemoryCacheAttribute.SetConfiguration(configuration.Object);
        
        var cacheKey = new TestMemoryCacheKey("test");

        _memoryCacheService
            .Setup(s => s.GetItem(cacheKey, typeof(TestValue)))
            .ReturnsAsync(null);

        var args = new List<object>();

        // We expect the cache configuration to be read from the ConfigurationSection.
        var expectedCacheConfiguration = new MemoryCacheConfiguration(35, CronExpression.Parse(HalfHourlyExpirySchedule));
            
        _memoryCacheService
            .Setup(s => s.SetItem(cacheKey, Capture.In(args), expectedCacheConfiguration, null))
            .Returns(Task.CompletedTask);

        var result = TestMethods.SpecificCacheConfigurationKey("test");

        VerifyAllMocks(_memoryCacheService);

        Assert.IsType<TestValue>(result);
        Assert.Equal(args[0], result);
    }

    [Fact]
    public void SpecificCacheConfigurationKey_CacheDurationInSecondsMissing()
    {
        var configuration = new Mock<IConfigurationSection>(MockBehavior.Strict);
        var specificCacheConfiguration = CreateMockConfigurationSection(
            TupleOf("CacheDurationInSeconds", (string) null));
        configuration
            .Setup(s => s.GetSection("SpecificCacheConfigurationKey"))
            .Returns(specificCacheConfiguration.Object);
        MemoryCacheAttribute.SetConfiguration(configuration.Object);
        
        var cacheKey = new TestMemoryCacheKey("test");

        _memoryCacheService
            .Setup(s => s.GetItem(cacheKey, typeof(TestValue)))
            .ReturnsAsync(null);

        var exception = Assert.Throws<ArgumentException>(() => TestMethods.SpecificCacheConfigurationKey("test"));
        Assert.Equal("A value for configuration MemoryCache.Configurations.CacheDurationInSeconds " +
                     "must be specified", exception.Message);
    }

    [Fact]
    public void SpecificCacheConfigurationKey_ExpiryScheduleMissing()
    {
        var configuration = new Mock<IConfigurationSection>(MockBehavior.Strict);
        var specificCacheConfiguration = CreateMockConfigurationSection(
            TupleOf("CacheDurationInSeconds", "35"),
            TupleOf("ExpirySchedule", (string) null));
        configuration
            .Setup(s => s.GetSection("SpecificCacheConfigurationKey"))
            .Returns(specificCacheConfiguration.Object);
        MemoryCacheAttribute.SetConfiguration(configuration.Object);
        
        var cacheKey = new TestMemoryCacheKey("test");
        
        _memoryCacheService
            .Setup(s => s.GetItem(cacheKey, typeof(TestValue)))
            .ReturnsAsync(null);

        var args = new List<object>();

        // We expect the cache configuration to be read from the ConfigurationSection with no ExpirySchedule.
        var expectedCacheConfiguration = new MemoryCacheConfiguration(35);
            
        _memoryCacheService
            .Setup(s => s.SetItem(cacheKey, Capture.In(args), expectedCacheConfiguration, null))
            .Returns(Task.CompletedTask);

        var result = TestMethods.SpecificCacheConfigurationKey("test");

        VerifyAllMocks(_memoryCacheService);

        Assert.IsType<TestValue>(result);
        Assert.Equal(args[0], result);
    }

    [Fact]
    public void UnspecifiedCacheConfigurationKey()
    {
        MemoryCacheAttribute.SetConfiguration(Mock.Of<IConfigurationSection>());
        
        var cacheKey = new TestMemoryCacheKey("test");

        _memoryCacheService
            .Setup(s => s.GetItem(cacheKey, typeof(TestValue)))
            .ReturnsAsync(null);

        var exception = Assert.Throws<ArgumentException>(() => TestMethods.UnspecifiedCacheConfigurationKey("test"));
        Assert.Equal("Could not find MemoryCache.Configurations entry with key " +
                     "UnspecifiedCacheConfigurationKey", exception.Message);
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