using GovUk.Education.ExploreEducationStatistics.Common.Cache.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Requests;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Cache;

public record ListPublicationsPostCacheKey(PublicationsListPostRequest PublicationQuery)
    : IMemoryCacheKey
{
    public string Key => $"{GetType().Name}:{PublicationQuery}";
}
