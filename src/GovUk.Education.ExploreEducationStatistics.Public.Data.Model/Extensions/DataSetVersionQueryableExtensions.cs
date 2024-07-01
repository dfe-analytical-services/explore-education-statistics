using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Extensions;

public static class DataSetVersionQueryableExtensions
{
    public static async Task<Either<ActionResult, DataSetVersion>> FindByVersion(
        this IQueryable<DataSetVersion> queryable,
        Guid dataSetId,
        string version,
        Func<IQueryable<DataSetVersion>, IQueryable<DataSetVersion>>? appendQuery = null,
        CancellationToken cancellationToken = default)
    {
        if (!VersionUtils.TryParse(version, out var semVersion))
        {
            return new NotFoundResult();
        }

        var query = queryable
            .Where(dsv => dsv.DataSetId == dataSetId)
            .Where(dsv => dsv.VersionMajor == semVersion.Major)
            .Where(dsv => dsv.VersionMinor == semVersion.Minor);

        return await (appendQuery is not null ? appendQuery(query) : query)
            .SingleOrNotFoundAsync(cancellationToken);
    }
}
