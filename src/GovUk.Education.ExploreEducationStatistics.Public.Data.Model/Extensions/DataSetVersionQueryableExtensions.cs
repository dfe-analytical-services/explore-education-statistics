using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using LinqToDB;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Extensions;

public static class DataSetVersionQueryableExtensions
{
    /// <summary>
    /// Returns specified data set version. If a wildcard is specified, the latest version of a data set based on a major/minor/patch level that's being wildcarded (i.e., substituted with '*') is returned.
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

        var query = queryable
            .Where(dsv => dsv.DataSetId == dataSetId);

        if (DataSetVersionWildcardHelper.ContainsWildcard(version))
        {
            query = query.WherePublishedStatus();
        }

        return await query
            .Where(v =>
                (!parsedVersion.Major.HasValue || v.VersionMajor == parsedVersion.Major) &&
                (!parsedVersion.Minor.HasValue || v.VersionMinor == parsedVersion.Minor) &&
                (!parsedVersion.Patch.HasValue || v.VersionPatch == parsedVersion.Patch))
            .OrderByDescending(v => v.VersionMajor)
            .ThenByDescending(v => v.VersionMinor)
            .ThenByDescending(v => v.VersionPatch)
            .FirstOrNotFoundAsync(cancellationToken);
    }

    /// <summary>
    /// Returns all previous patch versions of a data set for a given major and minor version.
    /// </summary>
    /// <param name="queryable">The queryable collection of <see cref="DataSetVersion"/> objects.</param>
    /// <param name="dataSetId">The unique identifier of the data set.</param>
    /// <param name="version">Data set version. Must have a patch version greater than 0.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A list of all previous patch versions for the specified major and minor version.</returns>
    /// <exception cref="ArgumentException">Thrown when the version contains a wildcard or patch version is 0.</exception>
    public static async Task<Either<ActionResult, DataSetVersion[]>> GetPreviousPatchVersions(
        this IQueryable<DataSetVersion> queryable,
        Guid dataSetId,
        string version,
        CancellationToken cancellationToken = default) =>
        DataSetVersionNumber.TryParse(version, out var versionNumber) 
            ? versionNumber.Patch == 0 ? throw new ArgumentException($"Patch version must be specified in version supplied ({version}).")
                : await queryable
                    .Where(dsv => dsv.DataSetId == dataSetId)
                    .WherePreviousPatchVersion(versionNumber)
                    .ToArrayAsync(cancellationToken) 
            : throw new ArgumentException($"Version supplied ({version}) is not a valid version number.");

    private static IQueryable<DataSetVersion> WherePreviousPatchVersion(
        this IQueryable<DataSetVersion> query,
        DataSetVersionNumber version) =>
        query.Where(v => v.VersionMajor == version.Major &&
                         v.VersionMinor == version.Minor &&
                         v.VersionPatch < version.Patch);
}
