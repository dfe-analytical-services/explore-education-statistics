using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Model.PublisherQueues;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Functions
{
    // ReSharper disable once UnusedType.Global
    public class PublishReleaseContentImmediateFunction
    {
        private readonly IContentService _contentService;
        private readonly INotificationsService _notificationsService;
        private readonly IReleaseService _releaseService;
        private readonly IReleaseStatusService _releaseStatusService;

        public PublishReleaseContentImmediateFunction(IContentService contentService,
            INotificationsService notificationsService,
            IReleaseService releaseService,
            IReleaseStatusService releaseStatusService)
        {
            _contentService = contentService;
            _notificationsService = notificationsService;
            _releaseService = releaseService;
            _releaseStatusService = releaseStatusService;
        }

        /**
         * Azure function which generates and publishes the content for a Release immediately.
         * Depends on the download files existing.
         * Sets the published time on the Release which means it's considered as 'Live'.
         */
        [FunctionName("PublishReleaseContentImmediate")]
        // ReSharper disable once UnusedMember.Global
        public async Task PublishReleaseContentImmediate(
            [QueueTrigger(PublishReleaseContentImmediateQueue)]
            PublishReleaseContentImmediateMessage message,
            ExecutionContext executionContext,
            ILogger logger)
        {
            logger.LogInformation($"{executionContext.FunctionName} triggered at: {DateTime.Now}");
            await UpdateStage(message, Stage.Started);
            try
            {
                await _contentService.UpdateContentAsync(new[] {message.ReleaseId}, false);
                await _releaseService.SetPublishedDateAsync(message.ReleaseId);
                await _notificationsService.NotifySubscribersAsync(new[] {message.ReleaseId});
                await UpdateStage(message, Stage.Complete);
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Exception occured while executing {executionContext.FunctionName}");
                await UpdateStage(message, Stage.Failed,
                    new ReleaseStatusLogMessage($"Exception publishing release immediately: {e.Message}"));
            }

            logger.LogInformation($"{executionContext.FunctionName} completed");
        }

        private async Task UpdateStage(PublishReleaseContentImmediateMessage message, Stage stage,
            ReleaseStatusLogMessage logMessage = null)
        {
            switch (stage)
            {
                case Stage.Started:
                    await _releaseStatusService.UpdateStagesAsync(message.ReleaseId,
                        message.ReleaseStatusId,
                        logMessage: logMessage,
                        publishingStage: ReleaseStatusPublishingStage.Started,
                        contentStage: ReleaseStatusContentStage.Started);
                    break;
                case Stage.Complete:
                    await _releaseStatusService.UpdateStagesAsync(message.ReleaseId,
                        message.ReleaseStatusId,
                        logMessage: logMessage,
                        publishingStage: ReleaseStatusPublishingStage.Complete,
                        contentStage: ReleaseStatusContentStage.Complete);
                    break;
                case Stage.Failed:
                    await _releaseStatusService.UpdateStagesAsync(message.ReleaseId,
                        message.ReleaseStatusId,
                        logMessage: logMessage,
                        publishingStage: ReleaseStatusPublishingStage.Failed,
                        contentStage: ReleaseStatusContentStage.Failed);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(stage), stage, null);
            }
        }

        private enum Stage
        {
            Started,
            Complete,
            Failed
        }
    }
}