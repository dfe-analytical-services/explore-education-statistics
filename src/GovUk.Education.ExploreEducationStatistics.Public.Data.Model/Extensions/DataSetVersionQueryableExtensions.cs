using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Extensions;

public static class DataSetVersionQueryableExtensions
{
    /// <summary>
    /// Returns specified data set version or the latest version of a data set based on a major/minor/patch level that can be wildcarded (i.e., substituted with '*').
    /// </summary>
    /// <param name="version">Data set version which can contain a wildcard</param>
    /// <param name="publicOnly">Specifies whether this method should return versions that are not
    /// "Published".
    /// </param>
    public static async Task<Either<ActionResult, DataSetVersion>> FindByVersion(
        this IQueryable<DataSetVersion> queryable,
        Guid dataSetId,
        string version,
        bool publicOnly = false,
        CancellationToken cancellationToken = default)
    {
        if (!DataSetVersionNumber.TryParse(version, out var parsedVersion))
        {
            return new NotFoundResult();
        }

        var query = queryable
            .Where(dsv => dsv.DataSetId == dataSetId);

        if (publicOnly)
        {
            query = query.Where(dsv => dsv.Status == DataSetVersionStatus.Published);
        }

        return await query.Where(v =>
                (!parsedVersion.Major.HasValue || v.VersionMajor == parsedVersion.Major) &&
                (!parsedVersion.Minor.HasValue || v.VersionMinor == parsedVersion.Minor) &&
                (!parsedVersion.Patch.HasValue || v.VersionPatch == parsedVersion.Patch))
            .OrderByDescending(v => v.VersionMajor)
            .ThenByDescending(v => v.VersionMinor)
            .ThenByDescending(v => v.VersionPatch)
            .FirstOrNotFoundAsync(cancellationToken);
    }
}
