using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Security.Extensions;

public static class DataSetAuthExtensions
{
    public static IQueryable<DataSet> WherePublicStatus(this IQueryable<DataSet> queryable)
        => queryable.Where(ds => ds.Status == DataSetStatus.Published
                                 || ds.Status == DataSetStatus.Deprecated);
}
