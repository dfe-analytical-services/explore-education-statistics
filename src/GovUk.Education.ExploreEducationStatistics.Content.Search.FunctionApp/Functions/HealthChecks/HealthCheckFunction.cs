﻿using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.HealthChecks.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.HealthChecks;

public class HealthCheckFunction
{
    [Function(nameof(HealthCheck))]
    [Produces("application/json")]
    public IActionResult HealthCheck(
#pragma warning disable IDE0060
        [HttpTrigger(AuthorizationLevel.Anonymous, "get")]
        HttpRequest request)
#pragma warning restore IDE0060
    {
        var healthCheckResponse = new HealthCheckResponseDto();

        if (healthCheckResponse.Healthy)
        {
            return new OkObjectResult(healthCheckResponse);
        }

        return new ObjectResult(healthCheckResponse) { StatusCode = StatusCodes.Status500InternalServerError };
    }
}
