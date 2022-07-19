#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Cache.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Services;

public class InMemoryCacheServiceTests
{
    private record SampleClassSuperclass;

    private record SampleClass : SampleClassSuperclass;

    private record SampleClassSubtype : SampleClass;

    private record SampleCacheKey(string Key) : IInMemoryCacheKey;

    [Fact]
    public async Task SetItem()
    {
        var cacheKey = new SampleCacheKey("Key");
        const string valueToCache = "test item";

        var memoryCache = new Mock<IMemoryCache>(Strict);

        var cacheConfiguration = new InMemoryCacheConfiguration(ExpirySchedule.Hourly, 45);
            
        var now = 
            new DateTime(
                    2022, 
                    07, 
                    18, 
                    0, 
                    29, 
                    30)
                .ToUniversalTime();
            
        var cacheEntry = new Mock<ICacheEntry>(Strict);

        var expectedCacheExpiry = now.AddSeconds(45);

        // Options that we expect to see set.
        cacheEntry
            .SetupSet(s => s.Size = $"\"{valueToCache}\"".Length);

        cacheEntry
            .SetupSet(s => s.AbsoluteExpiration = new DateTimeOffset(expectedCacheExpiry));

        // Default unset options.
        cacheEntry.SetupSet(s => s.AbsoluteExpirationRelativeToNow = null);
        cacheEntry.SetupSet(s => s.SlidingExpiration = null);
        cacheEntry.SetupSet(s => s.Priority = CacheItemPriority.Normal);
        cacheEntry.SetupSet(s => s.Value = valueToCache);
        cacheEntry.Setup(s => s.Dispose());
            
        memoryCache
            .Setup(mock => mock.CreateEntry(cacheKey))
            .Returns(() => cacheEntry.Object);

        var service = SetupService(memoryCache.Object);
        await service.SetItem(cacheKey, valueToCache, cacheConfiguration, now);
            
        VerifyAllMocks(memoryCache, cacheEntry);
    }
        
    [Fact]
    public async Task SetItem_ExpiryTimeNotTruncated_Hourly()
    {
        // The requested ExpirySchedule is hourly, meaning that cached items will have their 
        // expiry times truncated if the requested cache duration would carry over into a new
        // hour.
        var cacheConfiguration = new InMemoryCacheConfiguration(
            ExpirySchedule.Hourly, 
            45);

        // Set the current DateTime to be 29 minutes and 30 seconds past the hour.
        var now = 
            new DateTime(
                    2022, 
                    07, 
                    18, 
                    0, 
                    29, 
                    30)
                .ToUniversalTime();

        // Set the expected cache duration to be 45 seconds as per the requested duration.
        // This should not be truncated as it does not carry over into the next hour.
        var expectedCacheExpiry = now.AddSeconds(45);

        await SetItemAndAssertExpiryTime(now, cacheConfiguration, expectedCacheExpiry);
    }
        
    [Fact]
    public async Task SetItem_ExpiryTimeTruncated_Hourly()
    {
        // The requested ExpirySchedule is hourly, meaning that cached items will have their 
        // expiry times truncated if the requested cache duration would carry over into a new
        // hour.
        var cacheConfiguration = new InMemoryCacheConfiguration(
            ExpirySchedule.Hourly, 
            45);

        // Set the current DateTime to be 59 minutes and 30 seconds past the hour.
        var now = 
            new DateTime(
                    2022, 
                    07, 
                    18, 
                    0, 
                    59, 
                    30)
                .ToUniversalTime();

        // Set the expected cache duration to be 45 seconds as per the requested duration.
        // This should be truncated as it carries 15 seconds over into the next hour.
        var expectedCacheExpiry = now.AddSeconds(45 - 15);

        await SetItemAndAssertExpiryTime(now, cacheConfiguration, expectedCacheExpiry);
    }
        
    [Fact]
    public async Task SetItem_ExpiryTimeTruncated_Hourly_LastHourOfDay()
    {
        // The requested ExpirySchedule is hourly, meaning that cached items will have their 
        // expiry times truncated if the requested cache duration would carry over into a new
        // hour.
        var cacheConfiguration = new InMemoryCacheConfiguration(
            ExpirySchedule.Hourly, 
            45);

        // Set the current DateTime to be 59 minutes and 30 seconds past the hour during the last
        // hour of the day.
        var now = 
            new DateTime(
                    2022, 
                    07, 
                    18, 
                    23, 
                    59, 
                    30)
                .ToUniversalTime();

        // Set the expected cache duration to be 45 seconds as per the requested duration.
        // This should be truncated as it carries 15 seconds over into the next hour.
        var expectedCacheExpiry = now.AddSeconds(45 - 15);

        await SetItemAndAssertExpiryTime(now, cacheConfiguration, expectedCacheExpiry);
    }
        
    [Fact]
    public async Task SetItem_ExpiryTimeNotTruncated_HalfHourly()
    {
        // The requested ExpirySchedule is half-hourly, meaning that cached items will have
        // their expiry times truncated if the requested cache duration would carry over into
        // a new half-hour of the day.
        var cacheConfiguration = new InMemoryCacheConfiguration(
            ExpirySchedule.HalfHourly, 
            15);

        // Set the current DateTime to be 29 minutes and 30 seconds past the hour.
        var now = 
            new DateTime(
                    2022, 
                    07, 
                    18, 
                    0, 
                    29, 
                    30)
                .ToUniversalTime();

        // Set the expected cache duration to be 15 seconds as per the requested duration.
        // This should not be truncated as it does not carry over into the next half hour.
        var expectedCacheExpiry = now.AddSeconds(15);

        await SetItemAndAssertExpiryTime(now, cacheConfiguration, expectedCacheExpiry);
    }
        
    [Fact]
    public async Task SetItem_ExpiryTimeTruncated_HalfHourly()
    {
        // The requested ExpirySchedule is hourly, meaning that cached items will have their 
        // expiry times truncated if the requested cache duration would carry over into a new
        // hour.
        var cacheConfiguration = new InMemoryCacheConfiguration(
            ExpirySchedule.HalfHourly, 
            45);

        // Set the current DateTime to be 59 minutes and 30 seconds past the hour.
        var now = 
            new DateTime(
                    2022, 
                    07, 
                    18, 
                    0, 
                    29, 
                    30)
                .ToUniversalTime();

        // Set the expected cache duration to be 45 seconds as per the requested duration.
        // This should be truncated as it carries 15 seconds over into the next half hour.
        var expectedCacheExpiry = now.AddSeconds(45 - 15);

        await SetItemAndAssertExpiryTime(now, cacheConfiguration, expectedCacheExpiry);
    }
        
    [Fact]
    public async Task SetItem_ExpiryTimeTruncated_HalfHourly_LastHalfHourOfDay()
    {
        // The requested ExpirySchedule is hourly, meaning that cached items will have their 
        // expiry times truncated if the requested cache duration would carry over into a new
        // hour.
        var cacheConfiguration = new InMemoryCacheConfiguration(
            ExpirySchedule.HalfHourly, 
            45);

        // Set the current DateTime to be 59 minutes and 30 seconds past the hour during the last
        // half hour of the day.
        var now = 
            new DateTime(
                    2022, 
                    07, 
                    18, 
                    23, 
                    59, 
                    30)
                .ToUniversalTime();

        // Set the expected cache duration to be 45 seconds as per the requested duration.
        // This should be truncated as it carries 15 seconds over into the next half hour.
        var expectedCacheExpiry = now.AddSeconds(45 - 15);

        await SetItemAndAssertExpiryTime(now, cacheConfiguration, expectedCacheExpiry);
    }
        
    [Fact]
    public async Task SetItem_ExpiryTimeNotTruncated_NoExpirySchedule()
    {
        // The requested ExpirySchedule is hourly, meaning that cached items will have their 
        // expiry times truncated if the requested cache duration would carry over into a new
        // hour.
        var cacheConfiguration = new InMemoryCacheConfiguration(
            ExpirySchedule.None, 
            6000);

        var now = 
            new DateTime(
                    2022, 
                    07, 
                    18, 
                    0, 
                    29, 
                    30)
                .ToUniversalTime();

        // The requested cache duration is really long, but because there's no regular cache clearing
        // schedule, the requested cache duration will be honoured.
        var expectedCacheExpiry = now.AddSeconds(6000);

        await SetItemAndAssertExpiryTime(now, cacheConfiguration, expectedCacheExpiry);
    }

    private async Task SetItemAndAssertExpiryTime(
        DateTime now,
        InMemoryCacheConfiguration cacheConfiguration,
        DateTime expectedCacheExpiry)
    {
        var cacheKey = new SampleCacheKey("Key");
        const string valueToCache = "test item";

        var memoryCache = new Mock<IMemoryCache>(Strict);
        var cacheEntry = new Mock<ICacheEntry>(Strict);

        // Options that we expect to see set.
        cacheEntry
            .SetupSet(s => s.Size = $"\"{valueToCache}\"".Length);

        cacheEntry
            .SetupSet(s => s.AbsoluteExpiration = new DateTimeOffset(expectedCacheExpiry));

        // Default unset options.
        cacheEntry.SetupSet(s => s.AbsoluteExpirationRelativeToNow = null);
        cacheEntry.SetupSet(s => s.SlidingExpiration = null);
        cacheEntry.SetupSet(s => s.Priority = CacheItemPriority.Normal);
        cacheEntry.SetupSet(s => s.Value = valueToCache);
        cacheEntry.Setup(s => s.Dispose());
            
        memoryCache
            .Setup(mock => mock.CreateEntry(cacheKey))
            .Returns(() => cacheEntry.Object);

        var service = SetupService(memoryCache.Object);
        await service.SetItem(cacheKey, valueToCache, cacheConfiguration, now);
            
        VerifyAllMocks(memoryCache, cacheEntry);
    }

    [Fact]
    public async Task GetItem()
    {
        object entity = new SampleClass();

        var cacheKey = new SampleCacheKey("Key");

        var memoryCache = new Mock<IMemoryCache>(Strict);

        memoryCache
            .Setup(mock => mock.TryGetValue(cacheKey, out entity))
            .Returns(true);

        var service = SetupService(memoryCache.Object);

        var result = await service.GetItem(cacheKey, typeof(SampleClass));
        VerifyAllMocks(memoryCache);

        Assert.Equal(entity, result);
    }

    [Fact]
    public async Task GetItem_MoreSpecificSubtypeReturnsOk()
    {
        object entity = new SampleClassSubtype();

        var cacheKey = new SampleCacheKey("Key");

        var memoryCache = new Mock<IMemoryCache>(Strict);

        memoryCache
            .Setup(mock => mock.TryGetValue(cacheKey, out entity))
            .Returns(true);

        var service = SetupService(memoryCache.Object);

        var result = await service.GetItem(cacheKey, typeof(SampleClass));
        VerifyAllMocks(memoryCache);

        Assert.Equal(entity, result);
    }

    [Fact]
    public async Task GetItem_NullIfLessSpecificSupertype()
    {
        object entity = new SampleClassSuperclass();

        var cacheKey = new SampleCacheKey("Key");

        var memoryCache = new Mock<IMemoryCache>(Strict);

        memoryCache
            .Setup(mock => mock.TryGetValue(cacheKey, out entity))
            .Returns(true);

        var service = SetupService(memoryCache.Object);

        var result = await service.GetItem(cacheKey, typeof(SampleClass));
        VerifyAllMocks(memoryCache);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetItem_NullIfCacheMiss()
    {
        var cacheKey = new SampleCacheKey("Key");

        var memoryCache = new Mock<IMemoryCache>(Strict);

        object entity;
            
        memoryCache
            .Setup(mock => mock.TryGetValue(cacheKey, out entity))
            .Returns(false);

        var service = SetupService(memoryCache.Object);

        var result = await service.GetItem(cacheKey, typeof(SampleClass));
        VerifyAllMocks(memoryCache);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetItem_NullIfException()
    {
        var cacheKey = new SampleCacheKey("");

        var memoryCache = new Mock<IMemoryCache>(Strict);

        object entity;
        memoryCache
            .Setup(mock => mock.TryGetValue(cacheKey, out entity))
            .Throws(new Exception("Something went wrong"));

        var service = SetupService(memoryCache.Object);

        var result = await service.GetItem(cacheKey, typeof(SampleClass));
        VerifyAllMocks(memoryCache);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetItem_NullIfIncorrectType()
    {
        object entityOfIncorrectType = new SampleClass();

        var cacheKey = new SampleCacheKey("");

        var memoryCache = new Mock<IMemoryCache>(Strict);

        memoryCache
            .Setup(mock => mock.TryGetValue(cacheKey, out entityOfIncorrectType))
            .Returns(true);

        var service = SetupService(memoryCache.Object);

        var result = await service.GetItem(cacheKey, typeof(string));
        VerifyAllMocks(memoryCache);

        Assert.Null(result);
    }

    private static InMemoryCacheService SetupService(
        IMemoryCache? memoryCache = null,
        ILogger<InMemoryCacheService>? logger = null)
    {
        var service = new InMemoryCacheService(
            logger ?? Mock.Of<ILogger<InMemoryCacheService>>()
        );
        service.SetMemoryCache(memoryCache ?? Mock.Of<IMemoryCache>());
        return service;
    }
}