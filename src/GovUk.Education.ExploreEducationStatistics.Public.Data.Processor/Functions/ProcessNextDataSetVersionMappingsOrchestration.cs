using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Model;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions;

public static class ProcessNextDataSetVersionMappingsFunctionOrchestration
{
    [Function(nameof(ProcessNextDataSetVersionMappings))]
    public static async Task ProcessNextDataSetVersionMappings(
        [OrchestrationTrigger] TaskOrchestrationContext context,
        ProcessDataSetVersionContext input
    )
    {
        var logger = context.CreateReplaySafeLogger(nameof(ProcessNextDataSetVersionMappings));

        logger.LogInformation(
            "Processing next data set version (InstanceId={InstanceId}, DataSetVersionId={DataSetVersionId})",
            context.InstanceId,
            input.DataSetVersionId
        );

        try
        {
            await context.CallActivity(ActivityNames.CopyCsvFiles, logger, context.InstanceId);
            await context.CallActivity(ActivityNames.CreateMappings, logger, context.InstanceId);
            await context.CallActivity(ActivityNames.ApplyAutoMappings, logger, context.InstanceId);
            await context.CallActivity(
                ActivityNames.CompleteNextDataSetVersionMappingProcessing,
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
