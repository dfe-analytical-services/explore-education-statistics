using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Model;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions;

public static class LogRunningOrchestration
{
    [Function(nameof(ProcessLongRunningOrchestration))]
    public static async Task ProcessLongRunningOrchestration(
        [OrchestrationTrigger] TaskOrchestrationContext context,
        LongRunningOrchestrationContext input)
    {
        var logger = context.CreateReplaySafeLogger(nameof(ProcessLongRunningOrchestration));

        logger.LogInformation(
            "Processing long-running orchestration (InstanceId={InstanceId}, DurationSeconds={DurationSeconds})",
            context.InstanceId,
            input.DurationSeconds);

        try
        {
            await context.CallActivity(nameof(LongRunningFunctions.LongRunningActivity), logger, input);
        }
        catch (Exception e)
        {
            logger.LogError(e,
                "Activity failed with an exception (InstanceId={InstanceId}, DurationSeconds={DurationSeconds})",
                context.InstanceId,
                input.DurationSeconds);

            await context.CallActivity(ActivityNames.HandleProcessingFailure, logger, context.InstanceId);
        }
    }
}
