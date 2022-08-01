using GovUk.Education.ExploreEducationStatistics.Common.Cache.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Cache;

public record GetReleaseCacheKey(string PublicationSlug, string ReleaseSlug) : IMemoryCacheKey
{
    public string Key => $"{GetType().Name}:{PublicationSlug}/{ReleaseSlug}";
}