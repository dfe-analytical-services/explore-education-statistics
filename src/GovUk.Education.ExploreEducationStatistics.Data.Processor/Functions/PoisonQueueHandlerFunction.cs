#nullable enable
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Data.Processor.Model.ProcessorQueues;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Functions;

public class PoisonQueueHandlerFunction(
    IDataImportService dataImportService,
    ILogger<PoisonQueueHandlerFunction> logger
)
{
    [Function("ProcessUploadsPoisonHandler")]
    public async Task ProcessUploadsPoisonQueueHandler(
        [QueueTrigger(ImportsPendingPoisonQueue)] ImportMessage message,
        FunctionContext context
    )
    {
        try
        {
            await dataImportService.FailImport(
                message.Id,
                "File failed to import for unknown reason in upload processing stage."
            );
        }
        catch (Exception e)
        {
            logger.LogError(e, "Exception occured while executing {FunctionName}", context.FunctionDefinition.Name);
        }
    }
}
