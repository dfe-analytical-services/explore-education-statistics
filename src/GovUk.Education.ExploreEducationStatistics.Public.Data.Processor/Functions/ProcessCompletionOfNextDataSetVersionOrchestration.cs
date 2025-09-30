using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Model;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions;

public static class ProcessCompletionOfNextDataSetVersionOrchestration
{
    [Function(nameof(ProcessCompletionOfNextDataSetVersionImport))]
    public static async Task ProcessCompletionOfNextDataSetVersionImport(
        [OrchestrationTrigger] TaskOrchestrationContext context,
        ProcessDataSetVersionContext input
    )
    {
        var logger = context.CreateReplaySafeLogger(
            nameof(ProcessCompletionOfNextDataSetVersionImport)
        );

        logger.LogInformation(
            "Processing completion of import for next data set version (InstanceId={InstanceId}, "
                + "DataSetVersionId={DataSetVersionId})",
            context.InstanceId,
            input.DataSetVersionId
        );

        try
        {
            await context.CallActivityExclusively(
                ActivityNames.ImportMetadata,
                logger,
                context.InstanceId
            );
            await context.CallActivity(ActivityNames.CreateChanges, logger, context.InstanceId);
            await context.CallActivity(ActivityNames.ImportData, logger, context.InstanceId);
            await context.CallActivity(ActivityNames.WriteDataFiles, logger, context.InstanceId);
            await context.CallActivity(
                ActivityNames.CompleteNextDataSetVersionImportProcessing,
                logger,
                context.InstanceId
            );
        }
        catch (Exception e)
        {
            logger.LogError(
                e,
                "Activity failed with an exception (InstanceId={InstanceId}, DataSetVersionId={DataSetVersionId})",
                context.InstanceId,
                input.DataSetVersionId
            );

            await context.CallActivity(
                ActivityNames.HandleProcessingFailure,
                logger,
                context.InstanceId
            );
        }
    }
}
