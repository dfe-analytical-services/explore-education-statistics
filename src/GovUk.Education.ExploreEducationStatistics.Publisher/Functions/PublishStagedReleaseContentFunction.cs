using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Publisher.Utils;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Model.ReleaseStatusPublishingStage;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Functions
{
    // ReSharper disable once UnusedType.Global
    public class PublishStagedReleaseContentFunction
    {
        private readonly IBlobCacheService _blobCacheService;
        private readonly IContentService _contentService;
        private readonly INotificationsService _notificationsService;
        private readonly IReleaseStatusService _releaseStatusService;
        private readonly IPublishingService _publishingService;
        private readonly IReleaseService _releaseService;

        public PublishStagedReleaseContentFunction(IBlobCacheService blobCacheService,
            IContentService contentService,
            INotificationsService notificationsService,
            IReleaseStatusService releaseStatusService,
            IPublishingService publishingService,
            IReleaseService releaseService)
        {
            _blobCacheService = blobCacheService;
            _contentService = contentService;
            _notificationsService = notificationsService;
            _releaseStatusService = releaseStatusService;
            _publishingService = publishingService;
            _releaseService = releaseService;
        }

        /// <summary>
        /// Azure function which publishes the content for a Release at a scheduled time by moving it from a staging directory.
        /// </summary>
        /// <remarks>
        /// Sets the published time on the Release which means it's considered as 'Live'.
        /// </remarks>
        /// <param name="timer"></param>
        /// <param name="executionContext"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        [FunctionName("PublishStagedReleaseContent")]
        // ReSharper disable once UnusedMember.Global
        public async Task PublishStagedReleaseContent([TimerTrigger("%PublishReleaseContentCronSchedule%")]
            TimerInfo timer,
            ExecutionContext executionContext,
            ILogger logger)
        {
            logger.LogInformation("{0} triggered at: {1}",
                executionContext.FunctionName,
                DateTime.UtcNow);

            var scheduled = (await QueryScheduledReleases()).ToList();
            if (scheduled.Any())
            {
                var published = new List<ReleaseStatus>();
                foreach (var releaseStatus in scheduled)
                {
                    logger.LogInformation("Moving content for release: {0}",
                        releaseStatus.ReleaseId);
                    await UpdateStage(releaseStatus, Started);
                    try
                    {
                        await _publishingService.PublishStagedReleaseContent(releaseStatus.ReleaseId);

                        published.Add(releaseStatus);
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e, "Exception occured while executing {0}",
                            executionContext.FunctionName);
                        await UpdateStage(releaseStatus, Failed,
                            new ReleaseStatusLogMessage($"Exception in publishing stage: {e.Message}"));
                    }
                }

                var releaseIds = published.Select(status => status.ReleaseId).ToArray();

                try
                {
                    if (!PublisherUtils.IsDevelopment())
                    {
                        await _releaseService.DeletePreviousVersionsStatisticalData(releaseIds);
                    }

                    // Invalidate the 'All Methodologies' cache item in case any methodologies
                    // are now accessible for the first time after publishing these releases
                    await _blobCacheService.DeleteItem(new AllMethodologiesCacheKey());

                    await _contentService.DeletePreviousVersionsDownloadFiles(releaseIds);
                    await _contentService.DeletePreviousVersionsContent(releaseIds);
                    await _notificationsService.NotifySubscribers(releaseIds);
                    await UpdateStage(published, Complete);
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Exception occured while executing {0}",
                        executionContext.FunctionName);
                    await UpdateStage(published, Failed,
                        new ReleaseStatusLogMessage($"Exception in publishing stage: {e.Message}"));
                }
            }

            logger.LogInformation(
                "{0} completed. {1}",
                executionContext.FunctionName,
                timer.FormatNextOccurrences(1));
        }

        private async Task<IEnumerable<ReleaseStatus>> QueryScheduledReleases()
        {
            return await _releaseStatusService.GetWherePublishingDueTodayWithStages(
                content: ReleaseStatusContentStage.Complete,
                data: ReleaseStatusDataStage.Complete,
                publishing: Scheduled);
        }

        private async Task UpdateStage(IEnumerable<ReleaseStatus> releaseStatuses, ReleaseStatusPublishingStage stage,
            ReleaseStatusLogMessage logMessage = null)
        {
            foreach (var releaseStatus in releaseStatuses)
            {
                await UpdateStage(releaseStatus, stage, logMessage);
            }
        }

        private async Task UpdateStage(ReleaseStatus releaseStatus, ReleaseStatusPublishingStage stage,
            ReleaseStatusLogMessage logMessage = null)
        {
            await _releaseStatusService.UpdatePublishingStageAsync(releaseStatus.ReleaseId, releaseStatus.Id, stage,
                logMessage);
        }
    }
}
