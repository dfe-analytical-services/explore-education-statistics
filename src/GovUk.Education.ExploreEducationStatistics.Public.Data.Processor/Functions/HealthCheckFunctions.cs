using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions;

public class HealthCheckFunctions(
    ILogger<HealthCheckFunctions> logger,
    PublicDataDbContext publicDataDbContext)
{
    [Function(nameof(CountDataSets))]
    public async Task<string> CountDataSets(
        [ActivityTrigger] object? input,
        FunctionContext executionContext)
    {
        var message = $"Found {await publicDataDbContext.DataSets.CountAsync()} datasets.";
        logger.LogInformation(message);
        return message;
    }
    
    [Function(nameof(CheckForFileShareMount))]
    public async Task CheckForFileShareMount(
        [ActivityTrigger] object? input,
        FunctionContext executionContext)
    {
        logger.LogInformation("Attempting to read from file share");
        
        try
        {
            var fileShareMountExists = Directory.Exists("/data/public-api-parquet");

            if (fileShareMountExists)
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
    }
}
