using GovUk.Education.ExploreEducationStatistics.Common.Cache.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Cache;

public record ListThemesCacheKey : IMemoryCacheKey
{
    public string Key => GetType().Name;
}