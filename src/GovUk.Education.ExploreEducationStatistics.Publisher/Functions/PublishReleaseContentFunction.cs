using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Model.ReleaseStatusPublishingStage;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Functions
{
    // ReSharper disable once UnusedType.Global
    public class PublishReleaseContentFunction
    {
        private readonly INotificationsService _notificationsService;
        private readonly IReleaseStatusService _releaseStatusService;
        private readonly IPublishingService _publishingService;
        private readonly IReleaseService _releaseService;

        public PublishReleaseContentFunction(INotificationsService notificationsService,
            IReleaseStatusService releaseStatusService,
            IPublishingService publishingService,
            IReleaseService releaseService)
        {
            _notificationsService = notificationsService;
            _releaseStatusService = releaseStatusService;
            _publishingService = publishingService;
            _releaseService = releaseService;
        }

        /**
         * Azure function which publishes the content for a Release at a scheduled time by moving it from a staging directory.
         * Sets the published time on the Release which means it's considered as 'Live'.
         */
        [FunctionName("PublishReleaseContent")]
        // ReSharper disable once UnusedMember.Global
        public async Task PublishReleaseContent([TimerTrigger("%PublishReleaseContentCronSchedule%")]
            TimerInfo timer,
            ExecutionContext executionContext,
            ILogger logger)
        {
            logger.LogInformation($"{executionContext.FunctionName} triggered at: {DateTime.Now}");

            var scheduled = (await QueryScheduledReleases()).ToList();
            if (scheduled.Any())
            {
                var published = new List<ReleaseStatus>();
                foreach (var releaseStatus in scheduled)
                {
                    logger.LogInformation($"Moving content for release: {releaseStatus.ReleaseId}");
                    await UpdateStage(releaseStatus, Started);
                    try
                    {
                        await _publishingService.PublishStagedReleaseContentAsync(releaseStatus.ReleaseId);

                        published.Add(releaseStatus);
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e, $"Exception occured while executing {executionContext.FunctionName}");
                        await UpdateStage(releaseStatus, Failed,
                            new ReleaseStatusLogMessage($"Exception in publishing stage: {e.Message}"));
                    }
                }

                var releaseIds = published.Select(status => status.ReleaseId);

                try
                {
                    await _releaseService.RemoveDataForPreviousVersions(releaseIds);
                    await _notificationsService.NotifySubscribersAsync(releaseIds);
                    await UpdateStage(published, Complete);
                }
                catch (Exception e)
                {
                    logger.LogError(e, $"Exception occured while executing {executionContext.FunctionName}");
                    await UpdateStage(published, Failed,
                        new ReleaseStatusLogMessage($"Exception in publishing stage: {e.Message}"));
                }
            }

            logger.LogInformation(
                $"{executionContext.FunctionName} completed. {timer.FormatNextOccurrences(1)}");
        }

        private async Task<IEnumerable<ReleaseStatus>> QueryScheduledReleases()
        {
            var dateFilter = TableQuery.GenerateFilterConditionForDate(nameof(ReleaseStatus.Publish),
                QueryComparisons.LessThan, DateTime.Today.AddDays(1));
            var contentStageFilter = TableQuery.GenerateFilterCondition(nameof(ReleaseStatus.ContentStage),
                QueryComparisons.Equal, ReleaseStatusContentStage.Complete.ToString());
            var dataStageFilter = TableQuery.GenerateFilterCondition(nameof(ReleaseStatus.DataStage),
                QueryComparisons.Equal, ReleaseStatusDataStage.Complete.ToString());
            var publishingStageFilter = TableQuery.GenerateFilterCondition(nameof(ReleaseStatus.PublishingStage),
                QueryComparisons.Equal, Scheduled.ToString());

            var stageFilter = TableQuery.CombineFilters(
                TableQuery.CombineFilters(contentStageFilter, TableOperators.And, dataStageFilter),
                TableOperators.And, publishingStageFilter);
            var combinedFilter = TableQuery.CombineFilters(dateFilter, TableOperators.And, stageFilter);

            return await _releaseStatusService.ExecuteQueryAsync(new TableQuery<ReleaseStatus>().Where(combinedFilter));
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