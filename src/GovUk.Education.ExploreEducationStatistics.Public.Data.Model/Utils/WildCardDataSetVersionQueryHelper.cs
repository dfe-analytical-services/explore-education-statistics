using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Utils;

public interface IWildCardDataSetVersionQueryHelper
{
    public Task<Either<ActionResult, DataSetVersion>> GetDatasetVersionUsingWildCard(IQueryable<DataSetVersion> queryable, Guid dataSetId, string version, CancellationToken cancellationToken);
}

public class WildCardDataSetVersionQueryHelper : IWildCardDataSetVersionQueryHelper
{
    public async Task<Either<ActionResult, DataSetVersion>> GetDatasetVersionUsingWildCard(IQueryable<DataSetVersion> queryable, Guid dataSetId, string version, CancellationToken cancellationToken)
    {
        var parts = version.Trim('v').Split('.');
        int? major = parts[0] == "*" ? null : int.Parse(parts[0]);
        int? minor = (parts.Length > 1 && parts[1] != "*") ? int.Parse(parts[1]) : null;
        int? patch = (parts.Length > 2 && parts[2] != "*") ? int.Parse(parts[2]) : null;

        return await queryable
            .Where(dsv => dsv.DataSetId == dataSetId)
            .Where(v =>
                (!major.HasValue || v.VersionMajor == major) &&
                (!minor.HasValue || v.VersionMinor == minor) &&
                (!patch.HasValue || v.VersionPatch == patch))
            .OrderByDescending(v => v.VersionMajor)
            .ThenByDescending(v => v.VersionMinor)
            .ThenByDescending(v => v.VersionPatch)
            .FirstOrNotFoundAsync(cancellationToken);
    }
}
