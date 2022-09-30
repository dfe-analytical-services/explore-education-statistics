#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Model.PublisherQueues;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Functions;

public class PermalinksMigrationFunction
{
    private readonly IPermalinkMigrationService _permalinkMigrationService;

    public PermalinksMigrationFunction(IPermalinkMigrationService permalinkMigrationService)
    {
        _permalinkMigrationService = permalinkMigrationService;
    }

    /// <summary>
    /// Azure Function which enumerates all the permalink blobs in the permalinks storage container,
    /// and queues a message to migrate each individual permalink.
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
            await _permalinkMigrationService.EnumerateAllPermalinksForMigration();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to enumerate all permalinks");
        }

        logger.LogInformation("{functionName} completed", executionContext.FunctionName);
    }
}
