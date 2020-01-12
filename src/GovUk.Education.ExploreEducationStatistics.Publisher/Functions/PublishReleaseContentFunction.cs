using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Model.Stage;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Functions
{
    public class PublishReleaseContentFunction
    {
        private readonly INotificationsService _notificationsService;
        private readonly IReleaseStatusService _releaseStatusService;
        private readonly IPublishingService _publishingService;

        public PublishReleaseContentFunction(INotificationsService notificationsService,
            IReleaseStatusService releaseStatusService,
            IPublishingService publishingService)
        {
            _notificationsService = notificationsService;
            _releaseStatusService = releaseStatusService;
            _publishingService = publishingService;
        }

        [FunctionName("PublishReleaseContent")]
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
                        await _publishingService.PublishStagedContentAsync(releaseStatus);
                        published.Add(releaseStatus);
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e, $"Exception occured while executing {executionContext.FunctionName}");
                        await UpdateStage(releaseStatus, Failed);
                    }
                }

                var releaseIds = published.Select(status => status.ReleaseId);

                try
                {
                    await _notificationsService.NotifySubscribersAsync(releaseIds);
                    await UpdateStage(published, Complete);
                }
                catch (Exception e)
                {
                    logger.LogError(e, $"Exception occured while executing {executionContext.FunctionName}");
                    await UpdateStage(published, Failed);
                }
            }

            logger.LogInformation(
                $"{executionContext.FunctionName} completed. {timer.FormatNextOccurrences(1)}");
        }

        private async Task<IEnumerable<ReleaseStatus>> QueryScheduledReleases()
        {
            var dateQuery = TableQuery.GenerateFilterConditionForDate(nameof(ReleaseStatus.Publish),
                QueryComparisons.LessThan, DateTime.Today.AddDays(1));
            var contentStageQuery = TableQuery.GenerateFilterCondition(nameof(ReleaseStatus.ContentStage),
                QueryComparisons.Equal, Complete.ToString());
            var publishingStageQuery = TableQuery.GenerateFilterCondition(nameof(ReleaseStatus.PublishingStage),
                QueryComparisons.Equal, Scheduled.ToString());
            var stageQuery = TableQuery.CombineFilters(contentStageQuery, TableOperators.And, publishingStageQuery);
            var query = new TableQuery<ReleaseStatus>().Where(TableQuery.CombineFilters(dateQuery, TableOperators.And,
                stageQuery));

            return await _releaseStatusService.ExecuteQueryAsync(query);
        }

        private async Task UpdateStage(IEnumerable<ReleaseStatus> releaseStatuses, Stage stage)
        {
            foreach (var releaseStatus in releaseStatuses)
            {
                await UpdateStage(releaseStatus, stage);
            }
        }

        private async Task UpdateStage(ReleaseStatus releaseStatus, Stage stage)
        {
            await _releaseStatusService.UpdatePublishingStageAsync(releaseStatus.ReleaseId, releaseStatus.Id, stage);
        }
    }
}