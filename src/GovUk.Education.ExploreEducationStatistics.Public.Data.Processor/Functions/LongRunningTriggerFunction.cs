using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions;

public class LongRunningTriggerFunction(
    ILogger<LongRunningTriggerFunction> logger)
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
            nameof(LongRunningOrchestration.ProcessLongRunningOrchestration);

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
}
