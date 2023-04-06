#nullable enable
using System;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Model.PublisherQueues;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Functions;

/// <summary>
/// TODO EES-3755 Remove after Permalink snapshot migration work is complete
/// </summary>
public class PermalinksMigrationFunction
{
    private readonly ContentDbContext _contentDbContext;
    private readonly IStorageQueueService _storageQueueService;

    public PermalinksMigrationFunction(ContentDbContext contentDbContext,
        IStorageQueueService storageQueueService)
    {
        _contentDbContext = contentDbContext;
        _storageQueueService = storageQueueService;
    }

    /// <summary>
    /// Azure Function which enumerates all the permalinks and queues a message to migrate each individual permalink.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="executionContext"></param>
    /// <param name="logger"></param>
    [FunctionName("PermalinksMigration")]
    public async Task PermalinksMigration(
        [QueueTrigger(PermalinksMigrationQueue)]
        PermalinksMigrationMessage message,
        ExecutionContext executionContext,
        ILogger logger)
    {
        logger.LogInformation("{functionName} triggered", executionContext.FunctionName);

        try
        {
            await _contentDbContext.Permalinks
                .ToAsyncEnumerable()
                .ForEachAwaitAsync(async permalink =>
                {
                    await _storageQueueService.AddMessageAsync(PermalinkMigrationQueue,
                        new PermalinkMigrationMessage(permalink.Id));
                });
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to enumerate all permalinks");
        }

        logger.LogInformation("{functionName} completed", executionContext.FunctionName);
    }
}
