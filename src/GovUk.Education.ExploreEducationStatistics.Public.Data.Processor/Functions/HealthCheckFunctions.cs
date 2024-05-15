using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Functions;
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
        var connectionString = ConnectionUtils.GetPostgreSqlConnectionString("PublicDataDb");
        logger.LogInformation(connectionString);
        
        var message = $"Found {await publicDataDbContext.DataSets.CountAsync()} datasets.";
        logger.LogInformation(message);
        return message;
    }
    
    [Function(nameof(ListFileShareContents))]
    public async Task<string> ListFileShareContents(
        [ActivityTrigger] object? input,
        FunctionContext executionContext)
    {
        logger.LogInformation("Attempting to read from file share");
        
        try
        {
            var files = Directory.GetFiles("/data/public-api-parquet");
            var message = $"Found the following files in the file share:\n\n{files.JoinToString('\n')}";
            logger.LogInformation(message);
            return message;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error encountered when attempting to list files in the file share");
            throw;
        }
    }
}
