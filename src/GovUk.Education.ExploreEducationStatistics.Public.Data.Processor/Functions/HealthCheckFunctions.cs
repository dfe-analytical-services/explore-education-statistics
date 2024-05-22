using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Options;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions;

// ReSharper disable once ClassNeverInstantiated.Global
public class HealthCheckFunctions(
    ILogger<HealthCheckFunctions> logger,
    PublicDataDbContext publicDataDbContext,
    IOptions<DataFilesOptions> dataFilesOptions)
{
    [Function(nameof(CountDataSets))]
    public async Task<string> CountDataSets(
#pragma warning disable IDE0060
        [HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData request)
#pragma warning restore IDE0060
    {
        try
        {
            var message = $"Found {await publicDataDbContext.DataSets.CountAsync()} data sets.";
            logger.LogInformation(message);
            return message;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error encountered when querying Data Sets");
            throw;
        }
    }

    [Function(nameof(CheckForFileShareMount))]
    public Task CheckForFileShareMount(
#pragma warning disable IDE0060
        [HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData request)
#pragma warning restore IDE0060
    {
        logger.LogInformation("Attempting to read from file share");

        try
        {
            if (Directory.Exists(dataFilesOptions.Value.BasePath))
            {
                logger.LogInformation("Successfully found the file share mount");
            }
            else
            {
                logger.LogError("Unable to find the file share mount");
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error encountered when attempting to find the file share mount");
            throw;
        }

        return Task.CompletedTask;
    }
}
