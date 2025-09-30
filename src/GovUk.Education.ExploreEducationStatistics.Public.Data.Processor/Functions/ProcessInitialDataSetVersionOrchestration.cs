using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Model;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions;

public static class ProcessInitialDataSetVersionOrchestration
{
    [Function(nameof(ProcessInitialDataSetVersion))]
    public static async Task ProcessInitialDataSetVersion(
        [OrchestrationTrigger] TaskOrchestrationContext context,
        ProcessDataSetVersionContext input
    )
    {
        var logger = context.CreateReplaySafeLogger(nameof(ProcessInitialDataSetVersion));

        logger.LogInformation(
            "Processing initial data set version (InstanceId={InstanceId}, DataSetVersionId={DataSetVersionId})",
            context.InstanceId,
            input.DataSetVersionId
        );

        try
        {
            await context.CallActivity(ActivityNames.CopyCsvFiles, logger, context.InstanceId);
            await context.CallActivityExclusively(
                ActivityNames.ImportMetadata,
                logger,
                context.InstanceId
            );
            await context.CallActivity(ActivityNames.ImportData, logger, context.InstanceId);
            await context.CallActivity(ActivityNames.WriteDataFiles, logger, context.InstanceId);
            await context.CallActivity(
                ActivityNames.CompleteInitialDataSetVersionProcessing,
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
