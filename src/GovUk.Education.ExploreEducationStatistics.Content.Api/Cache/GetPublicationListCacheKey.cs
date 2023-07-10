using GovUk.Education.ExploreEducationStatistics.Common.Cache.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Requests;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Cache;

public record GetPublicationListCacheKey(PublicationsListRequest PublicationQuery) : IMemoryCacheKey
{
    public string Key => $"{GetType().Name}:{PublicationQuery}";
}