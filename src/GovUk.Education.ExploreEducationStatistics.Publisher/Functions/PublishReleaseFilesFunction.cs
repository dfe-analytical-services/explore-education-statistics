using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Model.PublisherQueues;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Model.ReleasePublishingStatusFilesStage;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Functions;

public class PublishReleaseFilesFunction(
    ILogger<PublishReleaseFilesFunction> logger,
    IPublishingService publishingService,
    IReleasePublishingStatusService releasePublishingStatusService,
    IPublishingCompletionService publishingCompletionService)
{
    /// <summary>
    /// Azure function which publishes the files for a Release by copying them between storage accounts.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    [Function("PublishReleaseFiles")]
    public async Task PublishReleaseFiles(
        [QueueTrigger(PublishReleaseFilesQueue)] PublishReleaseFilesMessage message,
        FunctionContext context)
    {
        logger.LogInformation("{FunctionName} triggered: {Message}",
            context.FunctionDefinition.Name,
            message);

        await UpdateFilesStage(message.ReleasePublishingKeys, Started);

        var successfulReleases = await message
            .ReleasePublishingKeys
            .ToAsyncEnumerable()
            .WhereAwait(async key =>
            {
                try
                {
                    await publishingService.PublishMethodologyFilesIfApplicableForRelease(
                        key.ReleaseVersionId);
                    await publishingService.PublishReleaseFiles(key.ReleaseVersionId);
                    return true;
                }
                catch (Exception e)
                {
                    logger.LogError(e,
                        "Exception occured while executing {FunctionName}",
                        context.FunctionDefinition.Name);

                    return false;
                }
            })
            .ToListAsync();

        var unsuccessfulReleases = message
            .ReleasePublishingKeys
            .Except(successfulReleases)
            .ToList();

        await UpdateFilesStage(successfulReleases, Complete);
        await UpdateFilesStage(unsuccessfulReleases, Failed);

        try
        {
            await publishingCompletionService.CompletePublishingIfAllPriorStagesComplete(successfulReleases);
        }
        catch (Exception e)
        {
            logger.LogError(e,
                "Exception occured while completing publishing in {FunctionName}",
                context.FunctionDefinition.Name);

            await UpdatePublishingStage(
                successfulReleases.Select(release =>
                    new ReleasePublishingKey(release.ReleaseVersionId, release.ReleaseStatusId)
                ).ToList(),
                ReleasePublishingStatusPublishingStage.Failed,
                new ReleasePublishingStatusLogMessage(
                    $"Failed during completion of the Publishing process: {e.Message}"));
        }

        logger.LogInformation("{FunctionName} completed", context.FunctionDefinition.Name);
    }

    private async Task UpdateFilesStage(
        IReadOnlyList<ReleasePublishingKey> releasePublishingKeys,
        ReleasePublishingStatusFilesStage stage,
        ReleasePublishingStatusLogMessage? logMessage = null)
    {
        await releasePublishingKeys
            .ToAsyncEnumerable()
            .ForEachAwaitAsync(key => releasePublishingStatusService.UpdateFilesStage(key, stage, logMessage));
    }

    private async Task UpdatePublishingStage(
        IReadOnlyList<ReleasePublishingKey> releasePublishingKeys,
        ReleasePublishingStatusPublishingStage stage,
        ReleasePublishingStatusLogMessage? logMessage = null)
    {
        await releasePublishingKeys
            .ToAsyncEnumerable()
            .ForEachAwaitAsync(key => releasePublishingStatusService.UpdatePublishingStage(key, stage, logMessage));
    }
}
