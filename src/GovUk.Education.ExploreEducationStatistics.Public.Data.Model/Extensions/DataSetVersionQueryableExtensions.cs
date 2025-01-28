using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using Microsoft.AspNetCore.Mvc;
using Semver;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Extensions;

public static class DataSetVersionQueryableExtensions
{
    public static async Task<Either<ActionResult, DataSetVersion>> FindByVersion(
        this IQueryable<DataSetVersion> queryable,
        Guid dataSetId,
        string version,
        CancellationToken cancellationToken = default)
    {
        return await (version.Contains('*') ? queryable.FindByWildcardVersion(dataSetId, version, cancellationToken) : queryable.FindBySpecificVersion(dataSetId, version, cancellationToken));
    }
    /// <summary>
    /// Returns the latest version of a data set based on a major/minor/patch level that's being wildcarded (i.e., substituted with '*').
    /// This method retrieves the most recent data set version based on which portion of the version (major, minor, or patch) is being wildcarded (i.e., the position of '*' within the string parameter wildcardedVersion)
    /// </summary>
    /// <param name="wildcardedVersion">Must contain * (representing the wildcard)</param>
    public static async Task<Either<ActionResult, DataSetVersion>> FindByWildcardVersion(
        this IQueryable<DataSetVersion> queryable,
        Guid dataSetId,
        string wildcardedVersion,
        CancellationToken cancellationToken = default)
    {
        SemVersion startRange, endRange;
        var dataSetVersion = wildcardedVersion.Trim('v');
        var parts = dataSetVersion.Split('.');
         
        if (!VersionUtils.TryParseWildcard(wildcardedVersion, out var semVersionRange)) return new NotFoundResult();
        else if (dataSetVersion == "*") startRange = endRange = new SemVersion(0);
        else
        {
            startRange = semVersionRange[0].Start;
            endRange = semVersionRange[0].End;
        }

        return await queryable
            .Where(dsv => dsv.DataSetId == dataSetId)
            .Where(v =>
                (endRange.Major == 0 ? true :
                    parts.ElementAtOrDefault(1) == "*" ? v.VersionMajor < endRange.Major :
                        v.VersionMajor == endRange.Major
                ) &&
                (endRange.Minor == 0 ? true :
                    parts.ElementAtOrDefault(2) == "*" ? v.VersionMinor < endRange.Minor :
                        v.VersionMinor == endRange.Minor
                ) &&
                (
                    endRange.Patch == 0 ? true : v.VersionPatch == endRange.Patch
                )
               )
            .OrderByDescending(v => v.VersionMajor)
            .ThenByDescending(v => v.VersionMinor)
            .ThenByDescending(v => v.VersionPatch)
            .FirstOrNotFoundAsync(cancellationToken);
    }
    public static async Task<Either<ActionResult, DataSetVersion>> FindBySpecificVersion(
       this IQueryable<DataSetVersion> queryable,
       Guid dataSetId,
       string version,
       CancellationToken cancellationToken = default)
    {
        if (!VersionUtils.TryParse(version, out var semVersion))
        {
            return new NotFoundResult();
        }

        return await queryable
            .Where(dsv => dsv.DataSetId == dataSetId)
            .Where(dsv => dsv.VersionMajor == semVersion.Major)
            .Where(dsv => dsv.VersionMinor == semVersion.Minor)
            .SingleOrNotFoundAsync(cancellationToken);
    }
}

