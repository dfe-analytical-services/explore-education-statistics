using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions;

public class CountDataSetsFunction(
    ILogger<CountDataSetsFunction> logger,
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
}
