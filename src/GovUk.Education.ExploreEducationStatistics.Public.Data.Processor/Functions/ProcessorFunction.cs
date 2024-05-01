using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Requests;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions;

public class ProcessorFunction
{
    [Function(nameof(ProcessorFunction))]
    public async Task<string> ProcessorOrchestration([OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var logger = context.CreateReplaySafeLogger(nameof(ProcessorOrchestration));

        logger.LogInformation("Running orchestration (Id={InstanceId})", context.InstanceId);

        return await context.CallActivityAsync<string>(nameof(CountDataSetsFunction.CountDataSets));
    }

    [Function(nameof(ProcessorTrigger))]
    public async Task<HttpResponseData> ProcessorTrigger(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "orchestrators/processor")]
        HttpRequestData req,
        [FromBody] ProcessorTriggerRequest processorTriggerRequest,
        [DurableClient] DurableTaskClient client,
        FunctionContext executionContext,
        CancellationToken cancellationToken)
    {
        var logger = executionContext.GetLogger(nameof(ProcessorTrigger));

        var instanceId = await client.ScheduleNewOrchestrationInstanceAsync(
            nameof(ProcessorFunction), processorTriggerRequest, cancellationToken);

        logger.LogInformation("Started orchestration (Id={InstanceId})", instanceId);

        return await client.CreateCheckStatusResponseAsync(req, instanceId, cancellation: cancellationToken);
    }
}
