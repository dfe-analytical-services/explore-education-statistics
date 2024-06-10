using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Requests;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Services.Interfaces;

public interface IDataSetService
{
    Task<Either<ActionResult, (Guid dataSetId, Guid dataSetVersionId)>> CreateInitialDataSetVersion(
        InitialDataSetVersionCreateRequest request,
        Guid instanceId,
        CancellationToken cancellationToken = default);

    Task<Either<ActionResult, (Guid dataSetId, Guid dataSetVersionId)>> CreateNextDataSetVersion(
        NextDataSetVersionCreateRequest request,
        Guid instanceId,
        CancellationToken cancellationToken = default);
}
