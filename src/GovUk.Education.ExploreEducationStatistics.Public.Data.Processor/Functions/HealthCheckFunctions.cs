using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Options;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions;

public class HealthCheckFunctions(
    ILogger<HealthCheckFunctions> logger,
    PublicDataDbContext publicDataDbContext,
    IOptions<ParquetFilesOptions> parquetFileOptions)
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
            if (Directory.Exists(parquetFileOptions.Value.BasePath))
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
