using Azure.Storage.Blobs;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Options;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions;

// ReSharper disable once ClassNeverInstantiated.Global
public class HealthCheckFunctions(
    ILogger<HealthCheckFunctions> logger,
    PublicDataDbContext publicDataDbContext,
    ContentDbContext contentDbContext,
    IDataSetVersionPathResolver dataSetVersionPathResolver,
    IOptions<AppOptions> appOptions
)
{
    [Function(nameof(HealthCheck))]
    [Produces("application/json")]
    public async Task<IActionResult> HealthCheck(
#pragma warning disable IDE0060
        [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest request)
#pragma warning restore IDE0060
    {
        var psqlConnectionHealthCheck = await CheckPsqlConnectionHealth();
        var fileShareMountHealthCheck = CheckFileShareMountHealth();
        var coreStorageConnectionHealthCheck = await CheckCoreStorageConnectionHealth();
        var contentDbConnectionHealthCheck = await CheckContentDbConnectionHealth();

        var healthCheckResponse = new HealthCheckResponse(
            PsqlConnection: psqlConnectionHealthCheck,
            FileShareMount: fileShareMountHealthCheck,
            CoreStorageConnection: coreStorageConnectionHealthCheck,
            ContentDbConnection: contentDbConnectionHealthCheck
        );

        if (healthCheckResponse.Healthy)
        {
            return new OkObjectResult(healthCheckResponse);
        }

        return new ObjectResult(healthCheckResponse) { StatusCode = StatusCodes.Status500InternalServerError };
    }

    private async Task<HealthCheckSummary> CheckPsqlConnectionHealth()
    {
        logger.LogInformation("Attempting to test PSQL connection health");

        try
        {
            await publicDataDbContext.DataSets.AnyAsync();
            return HealthCheckSummary.Healthy();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error encountered when testing PSQL connection health");
            return HealthCheckSummary.Unhealthy(e.Message);
        }
    }

    private HealthCheckSummary CheckFileShareMountHealth()
    {
        logger.LogInformation("Attempting to read from file share");

        try
        {
            if (Directory.Exists(dataSetVersionPathResolver.BasePath()))
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

    private async Task<HealthCheckSummary> CheckCoreStorageConnectionHealth()
    {
        logger.LogInformation("Attempting to test Core Storage connection health");

        try
        {
            var connectionString = appOptions.Value.PrivateStorageConnectionString;
            var blobClient = new BlobServiceClient(connectionString);
            var response = await blobClient.GetAccountInfoAsync();
            return response.HasValue
                ? HealthCheckSummary.Healthy()
                : HealthCheckSummary.Unhealthy("Could not retrieve Core Storage account info");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error encountered when testing Core Storage connection health");
            return HealthCheckSummary.Unhealthy(e.Message);
        }
    }

    private async Task<HealthCheckSummary> CheckContentDbConnectionHealth()
    {
        logger.LogInformation("Attempting to test Content DB connection health");

        try
        {
            await contentDbContext.Releases.AnyAsync();
            return HealthCheckSummary.Healthy();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error encountered when testing Content DB connection health");
            return HealthCheckSummary.Unhealthy(e.Message);
        }
    }

    public record HealthCheckResponse(
        HealthCheckSummary PsqlConnection,
        HealthCheckSummary FileShareMount,
        HealthCheckSummary CoreStorageConnection,
        HealthCheckSummary ContentDbConnection
    )
    {
        public bool Healthy =>
            PsqlConnection.IsHealthy
            && FileShareMount.IsHealthy
            && CoreStorageConnection.IsHealthy
            && ContentDbConnection.IsHealthy;
    }

    public record HealthCheckSummary
    {
        public bool IsHealthy { get; init; }
        public string? UnhealthyReason { get; init; }

        public static HealthCheckSummary Healthy()
        {
            return new HealthCheckSummary { IsHealthy = true };
        }

        public static HealthCheckSummary Unhealthy(string reason)
        {
            return new HealthCheckSummary { IsHealthy = false, UnhealthyReason = reason };
        }
    }
}
