using GovUk.Education.ExploreEducationStatistics.Common.Cache.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Requests;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Cache;

public record GetThemesCacheKey : IMemoryCacheKey
{
    public string Key => $"{GetType().Name}";
}