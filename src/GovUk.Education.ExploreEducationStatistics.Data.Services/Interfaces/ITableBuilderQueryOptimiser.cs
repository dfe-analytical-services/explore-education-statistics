using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;

public interface ITableBuilderQueryOptimiser
{
    Task<Either<ActionResult, bool>> IsCroppingRequired(FullTableQuery query);

    Task<Either<ActionResult, FullTableQuery>> CropQuery(FullTableQuery query, CancellationToken cancellationToken);
}
