using System;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Publisher.Configuration;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NCrontab;
using static GovUk.Education.ExploreEducationStatistics.Common.Utils.CronExpressionUtil;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Model.PublisherQueues;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Model.ReleasePublishingStatusContentStage;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Functions;

public class StageReleaseContentFunction(
    ILogger<StageReleaseContentFunction> logger,
    IOptions<AppSettingOptions> appSettingOptions,
    IContentService contentService,
    IReleasePublishingStatusService releasePublishingStatusService)
{
    private readonly AppSettingOptions _appSettingOptions = appSettingOptions.Value;

    /// <summary>
    /// Azure function which generates the content for a Release into a staging directory.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    [Function("StageReleaseContent")]
    public async Task StageReleaseContent(
        [QueueTrigger(StageReleaseContentQueue)]
        StageReleaseContentMessage message,
        FunctionContext context)
    {
        logger.LogInformation("{FunctionName} triggered: {Message}",
            context.FunctionDefinition.Name,
            message);
        await UpdateContentStage(message, Started);
        try
        {
            var publishStagedReleasesCronExpression = _appSettingOptions.PublishReleaseContentCronSchedule;
            var nextScheduledPublishingTime = CrontabSchedule.Parse(publishStagedReleasesCronExpression,
                new CrontabSchedule.ParseOptions
                {
                    IncludingSeconds = CronExpressionHasSecondPrecision(publishStagedReleasesCronExpression)
                }).GetNextOccurrence(DateTime.UtcNow);
            await contentService.UpdateContentStaged(nextScheduledPublishingTime,
                message.Releases.Select(tuple => tuple.ReleaseVersionId).ToArray());
            await UpdateContentStage(message, Scheduled);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Exception occured while executing {FunctionName}", context.FunctionDefinition.Name);

            await UpdateContentStage(message, Failed,
                new ReleasePublishingStatusLogMessage($"Exception in content stage: {e.Message}"));
        }

        logger.LogInformation("{FunctionName} completed", context.FunctionDefinition.Name);
    }

    private async Task UpdateContentStage(
        StageReleaseContentMessage message,
        ReleasePublishingStatusContentStage stage,
        ReleasePublishingStatusLogMessage? logMessage = null)
    {
        foreach (var (releaseVersionId, releaseStatusId) in message.Releases)
        {
            await releasePublishingStatusService.UpdateContentStageAsync(releaseVersionId: releaseVersionId,
                releaseStatusId: releaseStatusId,
                stage,
                logMessage);
        }
    }
}
