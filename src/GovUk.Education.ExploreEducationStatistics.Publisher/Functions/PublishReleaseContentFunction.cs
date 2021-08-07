using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Models;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Publisher.Utils;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Model.PublisherQueues;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Functions
{
    // ReSharper disable once UnusedType.Global
    public class PublishReleaseContentFunction
    {
        private readonly IBlobCacheService _blobCacheService;
        private readonly IContentService _contentService;
        private readonly INotificationsService _notificationsService;
        private readonly IReleaseService _releaseService;
        private readonly IReleaseStatusService _releaseStatusService;

        public PublishReleaseContentFunction(
            IBlobCacheService blobCacheService,
            IContentService contentService,
            INotificationsService notificationsService,
            IReleaseService releaseService,
            IReleaseStatusService releaseStatusService)
        {
            _blobCacheService = blobCacheService;
            _contentService = contentService;
            _notificationsService = notificationsService;
            _releaseService = releaseService;
            _releaseStatusService = releaseStatusService;
        }

        /// <summary>
        /// Azure function which generates and publishes the content for a Release immediately.
        /// </summary>
        /// <remarks>
        /// Depends on the download files existing.
        /// Sets the published time on the Release which means it's considered as 'Live'.
        /// </remarks>
        /// <param name="message"></param>
        /// <param name="executionContext"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        [FunctionName("PublishReleaseContent")]
        // ReSharper disable once UnusedMember.Global
        public async Task PublishReleaseContent(
            [QueueTrigger(PublishReleaseContentQueue)]
            PublishReleaseContentMessage message,
            ExecutionContext executionContext,
            ILogger logger)
        {
            logger.LogInformation("{0} triggered at: {1}",
                executionContext.FunctionName,
                DateTime.UtcNow);

            await UpdateStage(message.ReleaseId, message.ReleaseStatusId, State.Started);

            var context = new PublishContext(DateTime.UtcNow, false);

            try
            {
                await _contentService.UpdateContent(context, message.ReleaseId);
                await _releaseService.SetPublishedDates(message.ReleaseId, context.Published);

                if (!PublisherUtils.IsDevelopment())
                {
                    await _releaseService.DeletePreviousVersionsStatisticalData(message.ReleaseId);
                }

                // Invalidate the 'All Methodologies' cache item in case any methodologies
                // are now accessible for the first time after publishing this release
                await _blobCacheService.DeleteItem(new AllMethodologiesCacheKey());

                await _contentService.DeletePreviousVersionsDownloadFiles(message.ReleaseId);
                await _contentService.DeletePreviousVersionsContent(message.ReleaseId);
                await _notificationsService.NotifySubscribers(message.ReleaseId);
                await UpdateStage(message.ReleaseId, message.ReleaseStatusId, State.Complete);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Exception occured while executing {0}",
                    executionContext.FunctionName);
                logger.LogError("{StackTrace}", e.StackTrace);

                await UpdateStage(message.ReleaseId, message.ReleaseStatusId, State.Failed,
                    new ReleaseStatusLogMessage($"Exception publishing release immediately: {e.Message}"));
            }

            logger.LogInformation("{0} completed", executionContext.FunctionName);
        }

        private async Task UpdateStage(Guid releaseId, Guid releaseStatusId, State state,
            ReleaseStatusLogMessage logMessage = null)
        {
            switch (state)
            {
                case State.Started:
                    await _releaseStatusService.UpdateStagesAsync(releaseId,
                        releaseStatusId,
                        logMessage: logMessage,
                        publishing: ReleaseStatusPublishingStage.Started,
                        content: ReleaseStatusContentStage.Started);
                    break;
                case State.Complete:
                    await _releaseStatusService.UpdateStagesAsync(releaseId,
                        releaseStatusId,
                        logMessage: logMessage,
                        publishing: ReleaseStatusPublishingStage.Complete,
                        content: ReleaseStatusContentStage.Complete);
                    break;
                case State.Failed:
                    await _releaseStatusService.UpdateStagesAsync(releaseId,
                        releaseStatusId,
                        logMessage: logMessage,
                        publishing: ReleaseStatusPublishingStage.Failed,
                        content: ReleaseStatusContentStage.Failed);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        private enum State
        {
            Started,
            Complete,
            Failed
        }
    }
}
