using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Security.Extensions;

public static class DataSetVersionAuthExtensions
{
    public static IQueryable<DataSetVersion> WherePublicStatus(this IQueryable<DataSetVersion> queryable)
        => queryable.Where(ds => ds.Status == DataSetVersionStatus.Published
                                 || ds.Status == DataSetVersionStatus.Withdrawn
                                 || ds.Status == DataSetVersionStatus.Deprecated);
}