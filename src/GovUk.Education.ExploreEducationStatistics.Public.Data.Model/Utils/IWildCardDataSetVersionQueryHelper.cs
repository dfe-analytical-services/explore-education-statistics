using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Utils;

public interface IWildCardDataSetVersionQueryHelper
{
    public Task<Either<ActionResult, DataSetVersion>> GetDatasetVersionUsingWildCard(IQueryable<DataSetVersion> queryable, Guid dataSetId, string version, CancellationToken cancellationToken);
}
