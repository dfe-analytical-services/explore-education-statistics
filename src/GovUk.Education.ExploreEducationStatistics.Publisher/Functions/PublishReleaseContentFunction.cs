#nullable enable
using System;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Requests;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Models;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Publisher.Utils;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Model.PublisherQueues;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Functions
{
    // ReSharper disable once UnusedType.Global
    public class PublishReleaseContentFunction
    {
        private readonly ContentDbContext _contentDbContext;
        private readonly IBlobCacheService _blobCacheService;
        private readonly IContentService _contentService;
        private readonly INotificationsService _notificationsService;
        private readonly IReleaseService _releaseService;
        private readonly IReleasePublishingStatusService _releasePublishingStatusService;

        public PublishReleaseContentFunction(
            ContentDbContext contentDbContext,
            IBlobCacheService blobCacheService,
            IContentService contentService,
            INotificationsService notificationsService,
            IReleaseService releaseService,
            IReleasePublishingStatusService releasePublishingStatusService)
        {
            _contentDbContext = contentDbContext;
            _blobCacheService = blobCacheService;
            _contentService = contentService;
            _notificationsService = notificationsService;
            _releaseService = releaseService;
            _releasePublishingStatusService = releasePublishingStatusService;
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

                if (!EnvironmentUtils.IsLocalEnvironment())
                {
                    await _releaseService.DeletePreviousVersionsStatisticalData(message.ReleaseId);
                }

                // Invalidate the cached trees in case any methodologies/publications
                // are now accessible for the first time after publishing these releases
                await _blobCacheService.DeleteItem(new AllMethodologiesCacheKey());
                await _blobCacheService.DeleteItem(new PublicationTreeCacheKey());
                await _blobCacheService.DeleteItem(new PublicationTreeCacheKey(PublicationTreeFilter.AnyData));
                await _blobCacheService.DeleteItem(new PublicationTreeCacheKey(PublicationTreeFilter.LatestData));

                var release = await _contentDbContext.Releases
                    .Include(r => r.Publication)
                    .Where(r => r.Id == message.ReleaseId)
                    .SingleAsync();
                await _blobCacheService.DeleteItem(new PublicationCacheKey(release.Publication.Slug));
                // TODO: @MarkFix EES-3149 Delete superseded publication's cache here too?

                await _contentService.DeletePreviousVersionsDownloadFiles(message.ReleaseId);
                await _contentService.DeletePreviousVersionsContent(message.ReleaseId);

                await _notificationsService.NotifySubscribersIfApplicable(message.ReleaseId);

                await UpdateStage(message.ReleaseId, message.ReleaseStatusId, State.Complete);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Exception occured while executing {0}",
                    executionContext.FunctionName);
                logger.LogError("{StackTrace}", e.StackTrace);

                await UpdateStage(message.ReleaseId, message.ReleaseStatusId, State.Failed,
                    new ReleasePublishingStatusLogMessage($"Exception publishing release immediately: {e.Message}"));
            }

            logger.LogInformation("{0} completed", executionContext.FunctionName);
        }

        private async Task UpdateStage(Guid releaseId, Guid releaseStatusId, State state,
            ReleasePublishingStatusLogMessage? logMessage = null)
        {
            switch (state)
            {
                case State.Started:
                    await _releasePublishingStatusService.UpdateStagesAsync(releaseId,
                        releaseStatusId,
                        logMessage: logMessage,
                        publishing: ReleasePublishingStatusPublishingStage.Started,
                        content: ReleasePublishingStatusContentStage.Started);
                    break;
                case State.Complete:
                    await _releasePublishingStatusService.UpdateStagesAsync(releaseId,
                        releaseStatusId,
                        logMessage: logMessage,
                        publishing: ReleasePublishingStatusPublishingStage.Complete,
                        content: ReleasePublishingStatusContentStage.Complete);
                    break;
                case State.Failed:
                    await _releasePublishingStatusService.UpdateStagesAsync(releaseId,
                        releaseStatusId,
                        logMessage: logMessage,
                        publishing: ReleasePublishingStatusPublishingStage.Failed,
                        content: ReleasePublishingStatusContentStage.Failed);
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
