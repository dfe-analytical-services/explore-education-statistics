using System.Linq.Expressions;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Extensions;

public static class DataSetVersionAuthExtensions
{
    public static bool IsPublicStatus(this DataSetVersion dataSetVersion)
        => IsPublicStatus().Compile()(dataSetVersion);

    public static IQueryable<DataSetVersion> WherePublicStatus(this IQueryable<DataSetVersion> queryable)
        => queryable.Where(IsPublicStatus());

    private static Expression<Func<DataSetVersion, bool>> IsPublicStatus()
        => dataSetVersion => DataSetVersionStatusConstants.PublicStatuses.Contains(dataSetVersion.Status);
}
