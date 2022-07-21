#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Model.PublisherQueues;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Functions;

/**
 * Function used to migrate files in EES-3547
 * TODO Remove in EES-3552
 */
public class MigrateFileFunction
{
    private readonly IFileMigrationService _fileMigrationService;

    public MigrateFileFunction(
        IFileMigrationService fileMigrationService)
    {
        _fileMigrationService = fileMigrationService;
    }

    /// <summary>
    /// Azure Function which copies content type and size properties from a blob to a file
    /// </summary>
    /// <param name="message"></param>
    /// <param name="executionContext"></param>
    /// <param name="logger"></param>
    [FunctionName("MigrateFile")]
    public async Task MigrateFile(
        [QueueTrigger(MigrateFilesQueue)] MigrateFileMessage message,
        ExecutionContext executionContext,
        ILogger logger)
    {
        logger.LogInformation("{functionName} triggered: {message}",
            executionContext.FunctionName,
            message);

        try
        {
            var result = await _fileMigrationService.MigrateFile(message.Id);
            if (result.IsLeft)
            {
                var failure = result.Left;
                logger.LogError(
                    "Failed to migrate file {id}. Service returned {type}.",
                    message.Id,
                    failure.GetType().ShortDisplayName());
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to migrate file {id}", message.Id);
        }
    }
}
