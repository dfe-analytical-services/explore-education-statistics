using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Utils;
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
        if (version.Contains("*"))
            return await queryable.FindByWildcardVersion(dataSetId, version, cancellationToken);
        else
            return await queryable.FindBySpecificVersion(dataSetId, version, cancellationToken);
    }
    /// <summary>
    /// Finds the latest version of data set based on the position of '*' within the string parameter wildcardedVersion. 
    /// Checks which portion of the version (major, minor, or patch) is being wildcarded and based on that, it: Substitute 1 from the level above the level that's being wildcarded.
    /// This is because the end of the range that Semver (library used within VersionUtils) returns defaults to one level higher above the maximum version specified but with a pre-release flag (i.e., endRange.IsPrerelease is always true)
    /// </summary>
    /// <param name="wildcardedVersion">Must contain * (representing the wildcard)</param>
    public static async Task<Either<ActionResult, DataSetVersion>> FindByWildcardVersion(
        this IQueryable<DataSetVersion> queryable,
        Guid dataSetId,
        string wildcardedVersion,
        CancellationToken cancellationToken = default)
    {
        SemVersion startRange, endRange;
        var parts = wildcardedVersion.Trim('v').Split('.');
         
        if (!VersionUtils.TryParseWildcard(wildcardedVersion, out var semVersionRange))
            return new NotFoundResult();
        else if (wildcardedVersion.Trim('v') == "*")
            startRange = endRange = new SemVersion(0);
        else
        {
            startRange = semVersionRange[0].Start;
            endRange = semVersionRange[0].End;
        }

        return await queryable
            .Where(dsv => dsv.DataSetId == dataSetId)
            .Where(v =>
                (endRange.Major == 0 ? true :
                    parts.ElementAtOrDefault(1) == "*" ? v.VersionMajor == (endRange.Major - 1) :
                        v.VersionMajor == endRange.Major
                ) &&
                (endRange.Minor == 0 ? true :
                    parts.ElementAtOrDefault(2) == "*" ? v.VersionMinor == (endRange.Minor - 1) :
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

