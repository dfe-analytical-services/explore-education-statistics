using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Extensions;

public static class DataSetVersionQueryableExtensions
{
    /// <summary>
    /// Returns specified data set version, if a wildcard is specified, the latest version of a data set based on a major/minor/patch level that's being wildcarded (i.e., substituted with '*') is returned.
    /// </summary>
    /// <param name="queryable">The queryable collection of <see cref="DataSetVersion"/> objects.</param>
    /// <param name="version">Data set version which can contain a wildcard</param>
    /// <param name="dataSetId">The unique identifier of the data set.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    public static async Task<Either<ActionResult, DataSetVersion>> FindByVersion(
        this IQueryable<DataSetVersion> queryable,
        Guid dataSetId,
        string version,
        CancellationToken cancellationToken = default)
    {
        if (!DataSetVersionNumber.TryParse(version, out var parsedVersion))
        {
            return new NotFoundResult();
        }

        return await queryable
            .Where(dsv => dsv.DataSetId == dataSetId)
            .Where(v =>
                (!parsedVersion.Major.HasValue || v.VersionMajor == parsedVersion.Major) &&
                (!parsedVersion.Minor.HasValue || v.VersionMinor == parsedVersion.Minor) &&
                (!parsedVersion.Patch.HasValue || v.VersionPatch == parsedVersion.Patch))
            .OrderByDescending(v => v.VersionMajor)
            .ThenByDescending(v => v.VersionMinor)
            .ThenByDescending(v => v.VersionPatch)
            .FirstOrNotFoundAsync(cancellationToken);
    }
}
