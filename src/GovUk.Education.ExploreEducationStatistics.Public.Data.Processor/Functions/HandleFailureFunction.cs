using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Repository.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions;

public class HandleFailureFunction(
    ILogger<HandleFailureFunction> logger,
    IDataSetVersionImportRepository dataSetVersionImportRepository,
    IDataSetVersionRepository dataSetVersionRepository)
{
    [Function(nameof(HandleFailure))]
    public async Task HandleFailure([ActivityTrigger] Guid dataSetVersionId)
    {
        await dataSetVersionRepository.SetStatus(dataSetVersionId, DataSetVersionStatus.Failed);
        await dataSetVersionImportRepository.SetCompleted(dataSetVersionId, DateTimeOffset.UtcNow);
    }
}
