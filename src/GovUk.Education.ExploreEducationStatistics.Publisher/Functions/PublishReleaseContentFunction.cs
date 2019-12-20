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

        public PublishReleaseContentFunction(INotificationsService notificationsService,
            IReleaseStatusService releaseStatusService)
        {
            _notificationsService = notificationsService;
            _releaseStatusService = releaseStatusService;
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
                foreach (var releaseStatus in scheduled)
                {
                    logger.LogInformation($"Moving content for release: {releaseStatus.ReleaseId}");

                    await _releaseStatusService.UpdatePublishingStageAsync(releaseStatus.ReleaseId, releaseStatus.Id,
                        Started);

                    // TODO EES-865 Move content
                }

                var releaseIds = scheduled.Select(status => status.ReleaseId);
                await _notificationsService.NotifySubscribersAsync(releaseIds);

                foreach (var releaseStatus in scheduled)
                {
                    await _releaseStatusService.UpdatePublishingStageAsync(releaseStatus.ReleaseId, releaseStatus.Id,
                        Complete);
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
    }
}