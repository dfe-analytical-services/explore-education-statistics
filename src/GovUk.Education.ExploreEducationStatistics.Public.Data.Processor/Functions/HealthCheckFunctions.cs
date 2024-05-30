using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions;

// ReSharper disable once ClassNeverInstantiated.Global
public class HealthCheckFunctions(
    ILogger<HealthCheckFunctions> logger,
    PublicDataDbContext publicDataDbContext,
    IDataSetVersionPathResolver dataSetVersionPathResolver)
{
    [Function(nameof(HealthCheck))]
    [Produces("application/json")]
    public async Task<IActionResult> HealthCheck(
#pragma warning disable IDE0060
        [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest request)
#pragma warning restore IDE0060
    {
        var psqlConnectionHealthCheck = await CheckPsqlConnectionHealthy();
        var fileShareMountHealthCheck = CheckFileShareMountHealth();
        var healthCheckResponse = new HealthCheckResponse(
            PsqlConnection: psqlConnectionHealthCheck,
            FileShareMount: fileShareMountHealthCheck);

        if (healthCheckResponse.Healthy)
        {
            return new OkObjectResult(healthCheckResponse);
        }
        
        return new ObjectResult(healthCheckResponse){
            StatusCode = StatusCodes.Status500InternalServerError
        };
    }
    
    private async Task<HealthCheckSummary> CheckPsqlConnectionHealthy()
    {
        logger.LogInformation("Attempting to test PSQL health");

        try
        {
            await publicDataDbContext.DataSets.AnyAsync();
            return new HealthCheckSummary(true);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error encountered when testing PSQL connection health");
            return new HealthCheckSummary(false, e.Message);
        }
    }
    
    private HealthCheckSummary CheckFileShareMountHealth()
    {
        logger.LogInformation("Attempting to read from file share");
        
        try
        {
            if (Directory.Exists(dataSetVersionPathResolver.BasePath()))
            {
                return new HealthCheckSummary(true);
            }
            
            return new HealthCheckSummary(false, "File Share Mount folder does not exist");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error encountered when attempting to find the file share mount");
            return new HealthCheckSummary(false, e.Message);
        }
    }

    public record HealthCheckResponse(HealthCheckSummary PsqlConnection, HealthCheckSummary FileShareMount)
    {
        public bool Healthy => PsqlConnection.Healthy && FileShareMount.Healthy;
    };

    public record HealthCheckSummary(bool Healthy, string? UnhealthyReason = null);
}
