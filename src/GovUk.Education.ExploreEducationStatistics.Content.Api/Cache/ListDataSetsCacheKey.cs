using GovUk.Education.ExploreEducationStatistics.Common.Cache.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Requests;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Cache;

public record ListDataSetsCacheKey(DataSetsListRequest DataSetsQuery) : IMemoryCacheKey
{
    public string Key => $"{GetType().Name}:{DataSetsQuery}";
}
