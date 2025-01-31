using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Extensions;

public static class DataSetVersionQueryableExtensions
{
    /// <summary>
    /// Returns specified version or the latest version of a data set based on a major/minor/patch level that's being wildcarded (i.e., substituted with '*').
    /// based on which portion of the version (major, minor, or patch) is being wildcarded (i.e., the position of '*' within the string parameter wildcardedVersion)
    /// </summary>
    /// <param name="version">Must contain * (representing the wildcard)</param>
    public static async Task<Either<ActionResult, DataSetVersion>> FindByVersion(
        this IQueryable<DataSetVersion> queryable,
        Guid dataSetId,
        string version,
        CancellationToken cancellationToken = default)
    {
        if (!DataSetVersionRecord.TryParse(version, out var versionRecord)) 
            return new NotFoundResult();

        return await queryable
            .Where(dsv => dsv.DataSetId == dataSetId)
            .Where(v =>
                (!versionRecord.Major.HasValue || v.VersionMajor == versionRecord.Major) &&
                (!versionRecord.Minor.HasValue || v.VersionMinor == versionRecord.Minor) &&
                (!versionRecord.Patch.HasValue || v.VersionPatch == versionRecord.Patch))
            .OrderByDescending(v => v.VersionMajor)
            .ThenByDescending(v => v.VersionMinor)
            .ThenByDescending(v => v.VersionPatch)
            .FirstOrNotFoundAsync(cancellationToken);
    }
}

