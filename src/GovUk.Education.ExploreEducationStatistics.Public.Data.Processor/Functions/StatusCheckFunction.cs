using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions;

public class StatusCheckFunction
{
    private static readonly OrchestrationQuery ActiveOrchestrationsQuery = new()
    {
        Statuses =
        [
            OrchestrationRuntimeStatus.Pending,
            OrchestrationRuntimeStatus.Running
        ]
    };

    [Function(nameof(StatusCheck))]
    [Produces("application/json")]
    public static async Task<IActionResult> StatusCheck(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get")]
#pragma warning disable IDE0060
        HttpRequest request,
#pragma warning restore IDE0060
        [DurableClient] DurableTaskClient client)
    {
        var activeOrchestrations = await client
            .GetAllInstancesAsync(filter: ActiveOrchestrationsQuery)
            .CountAsync();

        return new OkObjectResult(new { ActiveOrchestrations = activeOrchestrations });
    }
}
