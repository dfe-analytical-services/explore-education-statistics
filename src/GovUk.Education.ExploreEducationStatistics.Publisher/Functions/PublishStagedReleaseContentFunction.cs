#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Publisher.Utils;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Model.ReleasePublishingStatusPublishingStage;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Functions
{
    // ReSharper disable once UnusedType.Global
    public class PublishStagedReleaseContentFunction
    {
        private readonly ContentDbContext _contentDbContext;
        private readonly IContentService _contentService;
        private readonly INotificationsService _notificationsService;
        private readonly IReleasePublishingStatusService _releasePublishingStatusService;
        private readonly IPublicationCacheService _publicationCacheService;
        private readonly IPublishingService _publishingService;
        private readonly IReleaseService _releaseService;

        public PublishStagedReleaseContentFunction(
            ContentDbContext contentDbContext,
            IContentService contentService,
            INotificationsService notificationsService,
            IReleasePublishingStatusService releasePublishingStatusService,
            IPublicationCacheService publicationCacheService,
            IPublishingService publishingService,
            IReleaseService releaseService)
        {
            _contentDbContext = contentDbContext;
            _contentService = contentService;
            _notificationsService = notificationsService;
            _releasePublishingStatusService = releasePublishingStatusService;
            _publicationCacheService = publicationCacheService;
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
                var publishedReleases = new List<ReleasePublishingStatus>();
                foreach (var releaseStatus in scheduled)
                {
                    await UpdateStage(releaseStatus, Started);
                    try
                    {
                        await _releaseService.SetPublishedDates(releaseStatus.ReleaseId, DateTime.UtcNow);
                        publishedReleases.Add(releaseStatus);
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e, "Exception occured while executing {0}",
                            executionContext.FunctionName);
                        await UpdateStage(releaseStatus, Failed,
                            new ReleasePublishingStatusLogMessage("Exception in publishing stage: " +
                                                                  $"{e.Message}\n{e.StackTrace}"));
                    }
                }

                try
                {
                    // Move all cached releases in the staging directory of the public content container to the root
                    await _publishingService.PublishStagedReleaseContent();

                    // Determine the distinct set of publications for the published releases
                    var publishedPublications = publishedReleases.Select(status => status.PublicationSlug)
                        .Distinct()
                        .ToList();

                    // Update the cached publications and any cached superseded publications.
                    // If any publications have a live release for the first time, the superseding is now enforced
                    var supersededPublications = await _contentDbContext.Publications
                        .Join(_contentDbContext.Publications,
                            publication => publication.Id,
                            supersededPublication => supersededPublication.SupersededById,
                            (publication, supersededPublication) => new { publication, supersededPublication })
                        .Where(tuple => publishedPublications.Contains(tuple.publication.Slug))
                        .Select(tuple => tuple.supersededPublication.Slug)
                        .ToListAsync();

                    var publicationsToUpdate = publishedPublications.Concat(supersededPublications);

                    await publicationsToUpdate
                        .ToAsyncEnumerable()
                        .ForEachAwaitAsync(_publicationCacheService.UpdatePublication);

                    var releaseIds = publishedReleases.Select(status => status.ReleaseId).ToArray();

                    if (!EnvironmentUtils.IsLocalEnvironment())
                    {
                        await _releaseService.DeletePreviousVersionsStatisticalData(releaseIds);
                    }

                    await _contentService.DeletePreviousVersionsDownloadFiles(releaseIds);
                    await _contentService.DeletePreviousVersionsContent(releaseIds);

                    await _notificationsService.NotifySubscribersIfApplicable(releaseIds);

                    // Update the cached trees in case any methodologies/publications
                    // are now accessible for the first time after publishing these releases
                    await _contentService.UpdateCachedTaxonomyBlobs();

                    await UpdateStage(publishedReleases, Complete);
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Exception occured while executing {0}",
                        executionContext.FunctionName);
                    await UpdateStage(publishedReleases, Failed,
                        new ReleasePublishingStatusLogMessage("Exception in publishing stage: " +
                                                              $"{e.Message}\n{e.StackTrace}"));
                }
            }

            logger.LogInformation(
                "{0} completed. {1}",
                executionContext.FunctionName,
                timer.FormatNextOccurrences(1));
        }

        private async Task<IEnumerable<ReleasePublishingStatus>> QueryScheduledReleases()
        {
            return await _releasePublishingStatusService.GetWherePublishingDueTodayWithStages(
                content: ReleasePublishingStatusContentStage.Complete,
                data: ReleasePublishingStatusDataStage.Complete,
                publishing: Scheduled);
        }

        private async Task UpdateStage(IEnumerable<ReleasePublishingStatus> releaseStatuses, ReleasePublishingStatusPublishingStage stage,
            ReleasePublishingStatusLogMessage? logMessage = null)
        {
            foreach (var releaseStatus in releaseStatuses)
            {
                await UpdateStage(releaseStatus, stage, logMessage);
            }
        }

        private async Task UpdateStage(ReleasePublishingStatus releasePublishingStatus, ReleasePublishingStatusPublishingStage stage,
            ReleasePublishingStatusLogMessage? logMessage = null)
        {
            await _releasePublishingStatusService.UpdatePublishingStageAsync(releasePublishingStatus.ReleaseId, releasePublishingStatus.Id, stage,
                logMessage);
        }
    }
}
