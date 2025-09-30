using GovUk.Education.ExploreEducationStatistics.Common.Cache.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Requests;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Cache;

public record ListDataSetFilesCacheKey(DataSetFileListRequest DataSetFileListRequest)
    : IMemoryCacheKey
{
    public string Key => $"{GetType().Name}:{DataSetFileListRequest}";
}
