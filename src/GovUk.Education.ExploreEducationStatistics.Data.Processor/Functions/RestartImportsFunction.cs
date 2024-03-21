#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.DataImportStatus;
using static GovUk.Education.ExploreEducationStatistics.Data.Processor.Model.ProcessorQueues;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Functions;

public class RestartImportsFunction(
    ILogger<RestartImportsFunction> logger,
    ContentDbContext contentDbContext,
    IStorageQueueService storageQueueService)
{
    private static readonly List<DataImportStatus> IncompleteStatuses =
    [
        QUEUED,
        PROCESSING_ARCHIVE_FILE,
        STAGE_1,
        STAGE_2,
        STAGE_3,
        CANCELLING
    ];

    [Function("RestartImports")]
    public async Task RestartImports(
        [QueueTrigger(RestartImportsQueue)] RestartImportsMessage message,
        FunctionContext context)
    {
        logger.LogInformation("{FunctionName} triggered", context.FunctionDefinition.Name);

        var incompleteImports = await contentDbContext
            .DataImports
            .Where(import => IncompleteStatuses.Contains(import.Status))
            .ToListAsync();

        var messages = incompleteImports
            .Select(import => new ImportMessage(import.Id))
            .ToList();

        await storageQueueService.AddMessages(ImportsPendingQueue, messages);

        logger.LogInformation("{FunctionName} completed", context.FunctionDefinition.Name);
    }
}
