using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Utils;

public static class DataSetVersionWildCardQueryHelper
{
    public static async Task<Either<ActionResult, DataSetVersion>> FetchDatasetUsingWildCardVersion(IQueryable<DataSetVersion> queryable, Guid dataSetId, string version, CancellationToken cancellationToken)
    {
        var parts = version.Trim('v').Split('.');
        Task<Either<ActionResult, DataSetVersion>> result;
        if (parts.Length == 2)
        {
            int? major = parts[0] == "*" ? null : int.Parse(parts[0]);
            int? minor = parts[1] == "*" ? null : int.Parse(parts[1]);

            result = queryable
                .Where(dsv => dsv.DataSetId == dataSetId)
                .Where(v =>
                (!major.HasValue || v.VersionMajor == major) &&
                (!minor.HasValue || v.VersionMinor == minor))
                .OrderByDescending(v => v.VersionMajor)
                .ThenByDescending(v => v.VersionMinor)
                .FirstOrNotFoundAsync(cancellationToken);
        }
        else if (parts.Length == 3)//REFACTOR TO REMOVE THIS BRANCHING based on testing
        {
            int? major = parts[0] == "*" ? null : int.Parse(parts[0]);
            int? minor = parts[1] == "*" ? null : int.Parse(parts[1]);
            int? patch = parts[2] == "*" ? null : int.Parse(parts[2]);

            result = queryable
                .Where(dsv => dsv.DataSetId == dataSetId)
                .Where(v =>
                    (!major.HasValue || v.VersionMajor == major) &&
                    (!minor.HasValue || v.VersionMinor == minor) &&
                    (!patch.HasValue || v.VersionPatch == patch))//TODO: find out if this is null if this errors
                .OrderByDescending(v => v.VersionMajor)
                .ThenByDescending(v => v.VersionMinor)
                .ThenByDescending(v => v.VersionPatch)//TODO: find out if this is null if this errors
                .FirstOrNotFoundAsync(cancellationToken);
        }
        else
        {
            return new NotFoundResult();
        }
        return await result;
    }
}
