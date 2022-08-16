#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Cache.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
using NCrontab;
using Xunit;
using static System.Globalization.DateTimeStyles;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Services;

public class MemoryCacheServiceTests
{
    private readonly CrontabSchedule _hourlyExpirySchedule = CrontabSchedule.Parse("0 * * * *");
    private readonly CrontabSchedule _halfHourlyExpirySchedule = CrontabSchedule.Parse("*/30 * * * *");

    private record SampleClassSuperclass;

    private record SampleClass : SampleClassSuperclass;

    private record SampleClassSubtype : SampleClass;

    private record SampleCacheKey(string Key) : IMemoryCacheKey;

    private readonly SampleCacheKey _cacheKey = new("Key");

    [Fact]
    public async Task SetItem()
    {
        // The requested ExpirySchedule is hourly, meaning that cached items will have their 
        // expiry times truncated if the requested cache duration would carry over into a new
        // hour.
        var cacheConfiguration = new MemoryCacheConfiguration(45, _hourlyExpirySchedule);

        // Set the current DateTime to be 29 minutes and 30 seconds past the hour.
        var now = DateTime.Parse("2022-07-18 00:29:30Z", styles: RoundtripKind);

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
        var cacheConfiguration = new MemoryCacheConfiguration(45, _hourlyExpirySchedule);

        // Set the current DateTime to be 59 minutes and 30 seconds past the hour.
        var now = DateTime.Parse("2022-07-18 00:59:30Z", styles: RoundtripKind);

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
        var cacheConfiguration = new MemoryCacheConfiguration(45, _hourlyExpirySchedule);

        // Set the current DateTime to be 59 minutes and 30 seconds past the hour during the last
        // hour of the day.
        var now = DateTime.Parse("2022-07-18 23:59:30Z", styles: RoundtripKind);

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
        var cacheConfiguration = new MemoryCacheConfiguration(15, _halfHourlyExpirySchedule);

        // Set the current DateTime to be 29 minutes and 30 seconds past the hour.
        var now = DateTime.Parse("2022-07-18 00:29:30Z", styles: RoundtripKind);

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
        var cacheConfiguration = new MemoryCacheConfiguration(45, _halfHourlyExpirySchedule);

        // Set the current DateTime to be 59 minutes and 30 seconds past the hour.
        var now = DateTime.Parse("2022-07-18 00:29:30Z", styles: RoundtripKind);

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
        var cacheConfiguration = new MemoryCacheConfiguration(45, _halfHourlyExpirySchedule);

        // Set the current DateTime to be 59 minutes and 30 seconds past the hour during the last
        // half hour of the day.
        var now = DateTime.Parse("2022-07-18 23:59:30Z", styles: RoundtripKind);

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
        var cacheConfiguration = new MemoryCacheConfiguration(6000);

        var now = DateTime.Parse("2022-07-18 00:29:30Z", styles: RoundtripKind);

        // The requested cache duration is really long, but because there's no regular cache clearing
        // schedule, the requested cache duration will be honoured.
        var expectedCacheExpiry = now.AddSeconds(6000);

        await SetItemAndAssertExpiryTime(now, cacheConfiguration, expectedCacheExpiry);
    }
        
    [Fact]
    public async Task SetItem_ExceptionHandledGracefullyWhenCachingItem()
    {
        var cacheConfiguration = new MemoryCacheConfiguration(6000);

        var memoryCache = new Mock<IMemoryCache>(Strict);

        memoryCache
            .Setup(mock => mock.CreateEntry(_cacheKey))
            .Throws(new Exception("Exception during \"SetItem\" call should have been handled gracefully"));

        var service = SetupService(memoryCache.Object);
        await service.SetItem(_cacheKey, "test item", cacheConfiguration, DateTime.UtcNow);
        VerifyAllMocks(memoryCache);
    }

    [Fact]
    public async Task GetItem()
    {
        object entity = new SampleClass();

        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        
        memoryCache.Set(_cacheKey, entity);

        var service = SetupService(memoryCache);

        var result = await service.GetItem(_cacheKey, typeof(SampleClass));

        Assert.Equal(entity, result);
    }

    [Fact]
    public async Task GetItem_MoreSpecificSubtypeReturnsOk()
    {
        object entity = new SampleClassSubtype();

        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        
        memoryCache.Set(_cacheKey, entity);

        var service = SetupService(memoryCache);

        var result = await service.GetItem(_cacheKey, typeof(SampleClass));

        Assert.Equal(entity, result);
    }

    [Fact]
    public async Task GetItem_NullIfLessSpecificSupertype()
    {
        object entity = new SampleClassSuperclass();
        
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        
        memoryCache.Set(_cacheKey, entity);

        var service = SetupService(memoryCache);

        var result = await service.GetItem(_cacheKey, typeof(SampleClass));

        Assert.Null(result);
    }

    [Fact]
    public async Task GetItem_NullIfCacheMiss()
    {
        var service = SetupService();

        var result = await service.GetItem(_cacheKey, typeof(SampleClass));

        Assert.Null(result);
    }

    [Fact]
    public async Task GetItem_NullIfException()
    {
        var key = new SampleCacheKey("");

        var memoryCache = new Mock<IMemoryCache>(Strict);

        object entity;
        memoryCache
            .Setup(mock => mock.TryGetValue(key, out entity))
            .Throws(new Exception("Something went wrong"));

        var service = SetupService(memoryCache.Object);

        var result = await service.GetItem(key, typeof(SampleClass));
        VerifyAllMocks(memoryCache);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetItem_NullIfIncorrectType()
    {
        object entityOfIncorrectType = new SampleClass();

        var key = new SampleCacheKey("");

        var memoryCache = new Mock<IMemoryCache>(Strict);

        memoryCache
            .Setup(mock => mock.TryGetValue(key, out entityOfIncorrectType))
            .Returns(true);

        var service = SetupService(memoryCache.Object);

        var result = await service.GetItem(key, typeof(string));
        VerifyAllMocks(memoryCache);

        Assert.Null(result);
    }

    private async Task SetItemAndAssertExpiryTime(
        DateTime now,
        MemoryCacheConfiguration cacheConfiguration,
        DateTime expectedCacheExpiry)
    {
        const string valueToCache = "test item";

        var memoryCache = new Mock<IMemoryCache>(Strict);
        var cacheEntry = new TestCacheEntry();

        memoryCache
            .Setup(mock => mock.CreateEntry(_cacheKey))
            .Returns(cacheEntry);

        var service = SetupService(memoryCache.Object);
        await service.SetItem(_cacheKey, valueToCache, cacheConfiguration, now);
        VerifyAllMocks(memoryCache);
        
        Assert.Equal($"\"{valueToCache}\"".Length, cacheEntry.Size);
        Assert.Equal(new DateTimeOffset(expectedCacheExpiry), cacheEntry.AbsoluteExpiration);
    }

    private static MemoryCacheService SetupService(
        IMemoryCache? memoryCache = null,
        ILogger<MemoryCacheService>? logger = null)
    {
        return new MemoryCacheService(
            memoryCache ?? new MemoryCache(new MemoryCacheOptions()),
            logger ?? Mock.Of<ILogger<MemoryCacheService>>()
        );
    }

    private record TestCacheEntry : ICacheEntry
    {
        public DateTimeOffset? AbsoluteExpiration { get; set; }
        public TimeSpan? AbsoluteExpirationRelativeToNow { get; set; }
        public IList<IChangeToken> ExpirationTokens { get; } = null!;
        public object Key { get; } = null!;
        public IList<PostEvictionCallbackRegistration> PostEvictionCallbacks { get; } = null!;
        public CacheItemPriority Priority { get; set; }
        public long? Size { get; set; }
        public TimeSpan? SlidingExpiration { get; set; }
        public object Value { get; set; } = null!;

        public void Dispose()
        {
        }
    }
 }