#nullable enable
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Data.Processor.Model.ProcessorQueues;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Functions;

public class RestartImportsFunction(
    ILogger<RestartImportsFunction> logger,
    ContentDbContext contentDbContext)
{
    [Function("RestartImports")]
    [QueueOutput(ImportsPendingQueue)]
    public async Task<ImportMessage[]> RestartImports(
#pragma warning disable IDE0060
        [QueueTrigger(RestartImportsQueue)] RestartImportsMessage message,
#pragma warning restore IDE0060
        FunctionContext context,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("{FunctionName} triggered", context.FunctionDefinition.Name);

        var restartableImports = await contentDbContext
            .DataImports
            .AsNoTracking()
            .Where(di => DataImportStatusExtensions.IncompleteStatuses.Contains(di.Status))
            .Select(di => new ImportMessage(di.Id))
            .ToArrayAsync(cancellationToken);

        logger.LogInformation("{FunctionName} completed", context.FunctionDefinition.Name);

        return restartableImports;
    }
}
