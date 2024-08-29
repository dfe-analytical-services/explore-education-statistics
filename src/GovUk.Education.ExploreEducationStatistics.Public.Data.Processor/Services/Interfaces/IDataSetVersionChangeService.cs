using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Services.Interfaces;

public interface IDataSetVersionChangeService
{
    Task<Either<ActionResult, Unit>> GenerateChanges(
        Guid nextDataSetVersionId,
        CancellationToken cancellationToken = default);
}
