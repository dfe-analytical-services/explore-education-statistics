using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Models;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Publisher.utils;
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

            var releaseStatusId = await GetReleaseStatusId(message);
            await UpdateStage(message.ReleaseId, releaseStatusId, Stage.Started);
            
            var context = new PublishContext(DateTime.UtcNow, false);
            
            try
            {
                await _contentService.UpdateContentAsync(new[] {message.ReleaseId}, context);
                await _releaseService.SetPublishedDatesAsync(message.ReleaseId, context.Published);
                
                if (!PublisherUtils.IsDevelopment())
                {
                    await _releaseService.DeletePreviousVersionsStatisticalData(new[] {message.ReleaseId});
                }
                
                await _contentService.DeletePreviousVersionsContent(new[] {message.ReleaseId});
                await _notificationsService.NotifySubscribersAsync(new[] {message.ReleaseId});
                await UpdateStage(message.ReleaseId, releaseStatusId, Stage.Complete);
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Exception occured while executing {executionContext.FunctionName}");
                await UpdateStage(message.ReleaseId, releaseStatusId, Stage.Failed,
                    new ReleaseStatusLogMessage($"Exception publishing release immediately: {e.Message}"));
            }

            logger.LogInformation($"{executionContext.FunctionName} completed");
        }

        private async Task UpdateStage(Guid releaseId, Guid releaseStatusId, Stage stage,
            ReleaseStatusLogMessage logMessage = null)
        {
            switch (stage)
            {
                case Stage.Started:
                    await _releaseStatusService.UpdateStagesAsync(releaseId,
                        releaseStatusId,
                        logMessage: logMessage,
                        publishing: ReleaseStatusPublishingStage.Started,
                        content: ReleaseStatusContentStage.Started);
                    break;
                case Stage.Complete:
                    await _releaseStatusService.UpdateStagesAsync(releaseId,
                        releaseStatusId,
                        logMessage: logMessage,
                        publishing: ReleaseStatusPublishingStage.Complete,
                        content: ReleaseStatusContentStage.Complete);
                    break;
                case Stage.Failed:
                    await _releaseStatusService.UpdateStagesAsync(releaseId,
                        releaseStatusId,
                        logMessage: logMessage,
                        publishing: ReleaseStatusPublishingStage.Failed,
                        content: ReleaseStatusContentStage.Failed);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(stage), stage, null);
            }
        }

        /**
         * ReleaseStatusId in the message is optional.
         * If it's set this function has been invoked in the context of all the other tasks with this ReleaseStatus.
         * If it's not set this function has been invoked directly for the purpose of regenerating and publishing just the content.
         * In this case we update the latest existing ReleaseStatus rather than creating a new one.
         */
        private async Task<Guid> GetReleaseStatusId(PublishReleaseContentImmediateMessage message)
        {
            if (message.ReleaseStatusId.HasValue)
            {
                return message.ReleaseStatusId.Value;
            }

            var releaseStatus = await _releaseStatusService.GetLatestAsync(message.ReleaseId);
            if (releaseStatus == null)
            {
                throw new InvalidOperationException(
                    $"Status not found for release: {message.ReleaseId}. Has it been approved?");
            }

            return releaseStatus.Id;
        }

        private enum Stage
        {
            Started,
            Complete,
            Failed
        }
    }
}