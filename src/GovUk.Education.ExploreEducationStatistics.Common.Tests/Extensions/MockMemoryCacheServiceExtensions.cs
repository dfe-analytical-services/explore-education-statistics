#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Cache.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using Moq;
using Moq.Language.Flow;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;

public static class MockMemoryCacheServiceExtensions
{
    public static void SetupNotFoundForAnyKey<TCacheKey, TItem>(this Mock<IMemoryCacheService> service)
        where TCacheKey : IMemoryCacheKey
        where TItem : class
    {
        service.SetupGetItemForAnyKeyReturnsNotFound<TCacheKey, TItem>();
        service.SetupSetItemForAnyKey<TCacheKey, TItem>();
    }

    public static IReturnsResult<IMemoryCacheService> SetupGetItemForAnyKeyReturnsNotFound<TCacheKey, TItem>(
        this Mock<IMemoryCacheService> service
    )
        where TCacheKey : IMemoryCacheKey
        where TItem : class
    {
        return service.Setup(s => s.GetItem(It.IsAny<TCacheKey>(), typeof(TItem))).Returns((object?)null);
    }

    public static ISetup<IMemoryCacheService> SetupSetItemForAnyKey<TCacheKey, TItem>(
        this Mock<IMemoryCacheService> service
    )
        where TCacheKey : IMemoryCacheKey
        where TItem : class
    {
        return service.Setup(s =>
            s.SetItem<object>(
                It.IsAny<TCacheKey>(),
                It.IsAny<TItem>(),
                It.IsAny<MemoryCacheConfiguration>(),
                It.IsAny<DateTimeOffset>()
            )
        );
    }
}
