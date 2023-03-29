#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Model.PublisherQueues;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Functions;

/// <summary>
/// TODO EES-3755 Remove after Permalink snapshot migration work is complete
/// </summary>
public class PermalinkMigrationFunction
{
    private readonly IPermalinkMigrationService _permalinkMigrationService;

    public PermalinkMigrationFunction(IPermalinkMigrationService permalinkMigrationService)
    {
        _permalinkMigrationService = permalinkMigrationService;
    }

    /// <summary>
    /// Azure Function which migrates a Permalink.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="executionContext"></param>
    /// <param name="logger"></param>
    [FunctionName("PermalinkMigration")]
    public async Task PermalinkMigration(
        [QueueTrigger(PermalinkMigrationQueue)]
        PermalinkMigrationMessage message,
        ExecutionContext executionContext,
        ILogger logger)
    {
        try
        {
            await _permalinkMigrationService.MigratePermalink(message.PermalinkId);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to migrate permalink {permalinkId}", message.PermalinkId);
        }
    }
}
