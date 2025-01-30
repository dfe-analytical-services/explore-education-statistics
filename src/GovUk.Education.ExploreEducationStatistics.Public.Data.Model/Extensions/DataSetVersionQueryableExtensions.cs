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
        CancellationToken cancellationToken = default) => await (version.Contains('*') 
            ? queryable.FindByWildcardVersion(dataSetId, version, cancellationToken) 
            : queryable.FindBySpecificVersion(dataSetId, version, cancellationToken));

    /// <summary>
    /// Returns the latest version of a data set based on a major/minor/patch level that's being wildcarded (i.e., substituted with '*').
    /// based on which portion of the version (major, minor, or patch) is being wildcarded (i.e., the position of '*' within the string parameter wildcardedVersion)
    /// </summary>
    /// <param name="versionString">Must contain * (representing the wildcard)</param>
    public static async Task<Either<ActionResult, DataSetVersion>> FindByWildcardVersion(
        this IQueryable<DataSetVersion> queryable,
        Guid dataSetId,
        string versionString,
        CancellationToken cancellationToken = default)
    {
        if (!WildcardVersion.TryParse(versionString, out var wildcardVersion)) 
            return new NotFoundResult();

        return await queryable
            .Where(dsv => dsv.DataSetId == dataSetId)
            .Where(v =>
                (!wildcardVersion.VersionMajor.HasValue || v.VersionMajor == wildcardVersion.VersionMajor) &&
                (!wildcardVersion.VersionMinor.HasValue || v.VersionMinor == wildcardVersion.VersionMinor) &&
                (!wildcardVersion.VersionPatch.HasValue || v.VersionPatch == wildcardVersion.VersionPatch))
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
            return new NotFoundResult();

        return await queryable
            .Where(dsv => dsv.DataSetId == dataSetId)
            .Where(dsv => dsv.VersionMajor == semVersion.Major)
            .Where(dsv => dsv.VersionMinor == semVersion.Minor)
            .SingleOrNotFoundAsync(cancellationToken);
    }
}

