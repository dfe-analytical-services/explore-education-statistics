using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Services.Interfaces;

public interface IDataSetVersionService
{
    Task<Either<ActionResult, (Guid dataSetId, Guid dataSetVersionId)>> CreateInitialVersion(
        Guid dataSetId,
        Guid releaseFileId,
        Guid instanceId,
        CancellationToken cancellationToken = default);
    
    Task<Either<ActionResult, (Guid dataSetId, Guid dataSetVersionId)>> CreateNextVersion(
        Guid dataSetId,
        Guid releaseFileId,
        Guid instanceId,
        CancellationToken cancellationToken = default);
    
    Task<Either<ActionResult, Unit>> DeleteVersion(
        Guid dataSetVersionId,
        CancellationToken cancellationToken = default);
}
