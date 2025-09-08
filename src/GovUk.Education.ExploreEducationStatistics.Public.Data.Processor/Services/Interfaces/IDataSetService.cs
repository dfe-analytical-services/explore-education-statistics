using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Requests;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Services.Interfaces;

public interface IDataSetService
{
    Task<Either<ActionResult, (Guid dataSetId, Guid dataSetVersionId)>> CreateDataSet(
        DataSetCreateRequest request,
        Guid instanceId,
        CancellationToken cancellationToken = default
    );
}
