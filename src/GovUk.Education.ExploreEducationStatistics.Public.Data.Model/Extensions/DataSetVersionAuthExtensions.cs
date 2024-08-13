using System.Linq.Expressions;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Extensions;

public static class DataSetVersionAuthExtensions
{
    private static readonly IReadOnlyList<DataSetVersionStatus> PublicStatuses = new List<DataSetVersionStatus>(
        [
            DataSetVersionStatus.Published,
            DataSetVersionStatus.Withdrawn,
            DataSetVersionStatus.Deprecated
        ]
    );

    public static bool IsPublicStatus(this DataSetVersion dataSetVersion)
        => IsPublicStatus().Compile()(dataSetVersion);

    public static IQueryable<DataSetVersion> WherePublicStatus(this IQueryable<DataSetVersion> queryable)
        => queryable.Where(IsPublicStatus());

    private static Expression<Func<DataSetVersion, bool>> IsPublicStatus()
        => dataSetVersion => PublicStatuses.Contains(dataSetVersion.Status);
}
