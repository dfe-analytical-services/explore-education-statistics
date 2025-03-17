using GovUk.Education.ExploreEducationStatistics.Analytics.Requests.Consumer.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Analytics.Requests.Consumer.Functions;

// ReSharper disable once ClassNeverInstantiated.Global
public class HealthCheckFunctions(
    ILogger<HealthCheckFunctions> logger,
    IAnalyticsPathResolver pathResolver)
{
    [Function(nameof(HealthCheck))]
    [Produces("application/json")]
    public Task<IActionResult> HealthCheck(
#pragma warning disable IDE0060
        [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest request)
#pragma warning restore IDE0060
    {
        var fileShareMountHealthCheck = CheckFileShareMountHealth();

        var healthCheckResponse = new HealthCheckResponse(
            FileShareMount: fileShareMountHealthCheck);

        if (healthCheckResponse.Healthy)
        {
            return Task.FromResult<IActionResult>(new OkObjectResult(healthCheckResponse));
        }
        
        return Task.FromResult<IActionResult>(new ObjectResult(healthCheckResponse) {
            StatusCode = StatusCodes.Status500InternalServerError
        });
    }
    
    private HealthCheckSummary CheckFileShareMountHealth()
    {
        logger.LogInformation("Attempting to read from file share");
        
        try
        {
            if (Directory.Exists(pathResolver.BasePath()))
            {
                return HealthCheckSummary.Healthy();
            }
            
            return HealthCheckSummary.Unhealthy("File Share Mount folder does not exist");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error encountered when attempting to find the file share mount");
            return HealthCheckSummary.Unhealthy(e.Message);
        }
    }

    private record HealthCheckResponse(HealthCheckSummary FileShareMount)
    {
        public bool Healthy => FileShareMount.IsHealthy;
    }

    public record HealthCheckSummary
    {
        public bool IsHealthy { get; private init; }
        public string? UnhealthyReason { get; init; }

        public static HealthCheckSummary Healthy()
        {
            return new HealthCheckSummary
            {
                IsHealthy = true
            };
        }

        public static HealthCheckSummary Unhealthy(string reason)
        {
            return new HealthCheckSummary
            {
                IsHealthy = false,
                UnhealthyReason = reason
            };
        }
    }
}
