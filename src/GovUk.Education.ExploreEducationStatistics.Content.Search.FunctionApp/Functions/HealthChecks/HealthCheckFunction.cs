using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.HealthChecks.Dtos;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.HealthChecks.Strategies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.HealthChecks;

public class HealthCheckFunction(IEnumerable<IHealthCheckStrategy> strategies)
{
    [Function(nameof(HealthCheck))]
    [Produces("application/json")]
    public async Task<IActionResult> HealthCheck(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get")] 
        #pragma warning disable IDE0060 // Suppress removing unused parameter - must have a valid binding name for Azure function
        HttpRequest httpRequest,
        CancellationToken cancellationToken)
    {
        // Execute strategies sequentially so as not to interleave the logs.
        var results = await strategies
            .ToAsyncEnumerable()
            .SelectAwait(async strategy => await strategy.Run(cancellationToken))
            .ToArrayAsync(cancellationToken: cancellationToken);
        
        var healthCheckResponse = new HealthCheckResponse
        {
            Results = results
        };

        return healthCheckResponse.IsHealthy
            ? new OkObjectResult(healthCheckResponse)
            : new ObjectResult(healthCheckResponse)
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
    }
}
