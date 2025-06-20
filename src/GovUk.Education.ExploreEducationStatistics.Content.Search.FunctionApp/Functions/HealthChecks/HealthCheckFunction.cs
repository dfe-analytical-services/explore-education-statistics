﻿using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.HealthChecks.Dtos;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.HealthChecks.Strategies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
#pragma warning disable IDE0060 // Suppress removing unused parameter `ignored` - must have a valid binding name for Azure function

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.HealthChecks;

public class HealthCheckFunction(IEnumerable<IHealthCheckStrategy> strategies, ILogger<HealthCheckFunction> logger)
{
    [Function(nameof(HealthCheck))]
    [Produces("application/json")]
    public Task<IActionResult> HealthCheck(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get")]
        HttpRequest httpRequest) =>
        Task.FromResult<IActionResult>(new OkResult());

    [Function(nameof(FullHealthCheck))]
    [Produces("application/json")]
    public async Task<IActionResult> FullHealthCheck(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get")] 
        HttpRequest ignored, //  The binding name _ is invalid
        CancellationToken cancellationToken)
    {
        // Execute strategies sequentially so as not to interleave the logs.
        var results = await strategies
            .ToAsyncEnumerable()
            .SelectAwait(async strategy => await Run(strategy))
            .Select(result => (HealthCheckResultViewModel) result)
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

        async Task<HealthCheckResult> Run(IHealthCheckStrategy strategy)
        {
            try
            {
                return await strategy.Run(cancellationToken);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error occurred whilst trying to run health check: {StrategyType}", strategy.GetType());
                return new HealthCheckResult(strategy, false, $"Exception: {e.Message}");
            }
        }
    }
}
