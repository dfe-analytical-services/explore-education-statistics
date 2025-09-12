using System.Linq.Expressions;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Extensions;

public static class DataSetVersionAuthExtensions
{
    public static readonly IReadOnlyList<DataSetVersionStatus> PublicStatuses = new List<DataSetVersionStatus>(
        [
            DataSetVersionStatus.Published,
            DataSetVersionStatus.Withdrawn,
            DataSetVersionStatus.Deprecated
        ]
    );

    public static readonly IReadOnlyList<DataSetVersionStatus> PrivateStatuses = EnumUtil
        .GetEnums<DataSetVersionStatus>()
        .Except(PublicStatuses)
        .ToList();

    public static bool IsPublicStatus(this DataSetVersion dataSetVersion)
        => IsPublicStatus().Compile()(dataSetVersion);

    public static IQueryable<DataSetVersion> WherePublicStatus(this IQueryable<DataSetVersion> queryable)
        => queryable.Where(IsPublicStatus());

    public static IQueryable<DataSetVersion> WherePublicStatusOrSpecifiedId(this IQueryable<DataSetVersion> queryable, Guid versionId)
        => queryable.Where(dataSetVersion =>
            PublicStatuses.Contains(dataSetVersion.Status)
            || dataSetVersion.Id == versionId);

    public static IQueryable<DataSetVersion> WherePublishedStatus(this IQueryable<DataSetVersion> queryable)
        => queryable.Where(dv => dv.Status == DataSetVersionStatus.Published);
    
    private static Expression<Func<DataSetVersion, bool>> IsPublicStatus()
        => dataSetVersion => PublicStatuses.Contains(dataSetVersion.Status);

    public static bool IsPrivateStatus(this DataSetVersion dataSetVersion)
        => IsPrivateStatus().Compile()(dataSetVersion);

    public static IQueryable<DataSetVersion> WherePrivateStatus(this IQueryable<DataSetVersion> queryable)
        => queryable.Where(IsPrivateStatus());

    private static Expression<Func<DataSetVersion, bool>> IsPrivateStatus()
        => dataSetVersion => PrivateStatuses.Contains(dataSetVersion.Status);
}
