using System;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using NCrontab;
using static GovUk.Education.ExploreEducationStatistics.Common.Utils.CronExpressionUtil;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Model.PublisherQueues;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Model.ReleasePublishingStatusContentStage;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Functions
{
    // ReSharper disable once UnusedType.Global
    public class StageReleaseContentFunction
    {
        private readonly IContentService _contentService;
        private readonly IReleasePublishingStatusService _releasePublishingStatusService;

        public StageReleaseContentFunction(IContentService contentService,
            IReleasePublishingStatusService releasePublishingStatusService)
        {
            _contentService = contentService;
            _releasePublishingStatusService = releasePublishingStatusService;
        }

        /// <summary>
        /// Azure function which generates the content for a Release into a staging directory.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="executionContext"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        [FunctionName("StageReleaseContent")]
        // ReSharper disable once UnusedMember.Global
        public async Task StageReleaseContent(
            [QueueTrigger(StageReleaseContentQueue)] StageReleaseContentMessage message,
            ExecutionContext executionContext,
            ILogger logger)
        {
            logger.LogInformation("{FunctionName} triggered: {Message}",
                executionContext.FunctionName,
                message);
            await UpdateContentStage(message, Started);
            try
            {
                var publishStagedReleasesCronExpression = Environment.GetEnvironmentVariable("PublishReleaseContentCronSchedule") ?? "";
                var nextScheduledPublishingTime = CrontabSchedule.Parse(publishStagedReleasesCronExpression, new CrontabSchedule.ParseOptions
                {
                    IncludingSeconds = CronExpressionHasSecondPrecision(publishStagedReleasesCronExpression)
                }).GetNextOccurrence(DateTime.UtcNow);
                await _contentService.UpdateContentStaged(nextScheduledPublishingTime,
                    message.Releases.Select(tuple => tuple.ReleaseId).ToArray());
                await UpdateContentStage(message, Scheduled);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Exception occured while executing {FunctionName}",
                    executionContext.FunctionName);

                await UpdateContentStage(message, Failed,
                    new ReleasePublishingStatusLogMessage($"Exception in content stage: {e.Message}"));
            }

            logger.LogInformation("{FunctionName} completed", executionContext.FunctionName);
        }

        private async Task UpdateContentStage(
            StageReleaseContentMessage message,
            ReleasePublishingStatusContentStage stage,
            ReleasePublishingStatusLogMessage logMessage = null)
        {
            foreach (var (releaseId, releaseStatusId) in message.Releases)
            {
                await _releasePublishingStatusService.UpdateContentStageAsync(releaseId, releaseStatusId, stage, logMessage);
            }
        }
    }
}
