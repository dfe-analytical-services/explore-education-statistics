using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Repository.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions;

public class CompleteImportFunction(
    ILogger<CompleteImportFunction> logger,
    IDataSetVersionImportRepository dataSetVersionImportRepository,
    IDataSetVersionRepository dataSetVersionRepository)
{
    [Function(nameof(CompleteImport))]
    public async Task CompleteImport(
        [ActivityTrigger] Guid dataSetVersionId,
        FunctionContext executionContext)
    {
        await dataSetVersionImportRepository.UpdateStage(dataSetVersionId, DataSetVersionImportStage.Completing);

        // Any additional logic to tidy up after importing will be added here

        await dataSetVersionRepository.UpdateStatus(dataSetVersionId, DataSetVersionStatus.Draft);
    }
}
