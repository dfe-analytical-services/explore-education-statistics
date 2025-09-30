using System.Diagnostics;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions;

public class LongRunningFunctions(ILogger<LongRunningFunctions> logger)
{
    [Function(nameof(TriggerLongRunningOrchestration))]
    public async Task<IActionResult> TriggerLongRunningOrchestration(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = nameof(TriggerLongRunningOrchestration))]
        HttpRequest httpRequest,
        [DurableClient] DurableTaskClient client,
        CancellationToken cancellationToken)
    {
        var instanceId = Guid.NewGuid();

        var durationSeconds =
            httpRequest.GetRequestParamInt("durationSeconds", defaultValue: 60);

        const string orchestratorName =
            nameof(ProcessLongRunningOrchestration);

        var options = new StartOrchestrationOptions { InstanceId = instanceId.ToString() };

        logger.LogInformation(
            "Scheduling '{OrchestratorName}' (InstanceId={InstanceId}, DurationSeconds={DurationSeconds}))",
            orchestratorName,
            instanceId,
            durationSeconds);

        await client.ScheduleNewOrchestrationInstanceAsync(
            orchestratorName,
            new LongRunningOrchestrationContext { DurationSeconds = durationSeconds },
            options,
            cancellationToken);

        return new OkResult();
    }

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
            await context.CallActivity(nameof(LongRunningActivity), logger, input);
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

    [Function(nameof(LongRunningActivity))]
    public async Task LongRunningActivity(
        [ActivityTrigger] LongRunningOrchestrationContext input,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        while (stopwatch.Elapsed.Seconds < input.DurationSeconds)
        {
            await Task.Delay(10000, cancellationToken);

            logger.LogInformation($"Long-running orchestration running for {stopwatch.Elapsed.Seconds} " +
                                  $"out of {input.DurationSeconds} seconds");
        }
    }
}
