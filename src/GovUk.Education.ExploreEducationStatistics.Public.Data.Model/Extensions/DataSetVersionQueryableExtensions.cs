using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Utils;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Extensions;

public static class DataSetVersionQueryableExtensions
{
    public static async Task<Either<ActionResult, DataSetVersion>> FindByVersion(
        this IQueryable<DataSetVersion> queryable,
        Guid dataSetId,
        string version,
        IWildCardDataSetVersionQueryHelper wildcardDataSetVersionQueryHelper,
        CancellationToken cancellationToken = default)
    {
        if (version.Contains("*")) 
        {
            return await wildcardDataSetVersionQueryHelper.GetDatasetVersionUsingWildCard(queryable, dataSetId, version, cancellationToken);
        }

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
