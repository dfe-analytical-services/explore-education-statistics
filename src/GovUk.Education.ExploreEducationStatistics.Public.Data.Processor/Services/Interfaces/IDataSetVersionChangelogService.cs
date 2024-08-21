using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Services.Interfaces;

public interface IDataSetVersionChangelogService
{
    Task<Either<ActionResult, Unit>> GenerateChangelog(
        Guid nextDataSetVersionId,
        CancellationToken cancellationToken = default);
}
