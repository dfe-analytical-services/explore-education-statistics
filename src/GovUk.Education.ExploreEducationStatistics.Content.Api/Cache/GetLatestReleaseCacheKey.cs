using GovUk.Education.ExploreEducationStatistics.Common.Cache.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Cache;

public record GetLatestReleaseCacheKey(string PublicationSlug) : IMemoryCacheKey
{
    public string Key => $"{GetType().Name}:{PublicationSlug}";
}