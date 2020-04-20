using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Model.ReleaseStatusStates;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Functions
{
    // ReSharper disable once UnusedType.Global
    public class PublishReleasesFunction
    {
        private readonly IQueueService _queueService;
        private readonly IReleaseStatusService _releaseStatusService;

        public PublishReleasesFunction(IQueueService queueService, IReleaseStatusService releaseStatusService)
        {
            _queueService = queueService;
            _releaseStatusService = releaseStatusService;
        }

        /**
         * Azure function which triggers tasks for all Releases that are scheduled to be published later during the day.
         * Triggers publishing files and statistics data.
         */
        [FunctionName("PublishReleases")]
        // ReSharper disable once UnusedMember.Global
        public void PublishReleases([TimerTrigger("%PublishReleasesCronSchedule%")]
            TimerInfo timer,
            ExecutionContext executionContext,
            ILogger logger)
        {
            logger.LogInformation($"{executionContext.FunctionName} triggered at: {DateTime.Now}");
            PublishReleases().Wait();
            logger.LogInformation(
                $"{executionContext.FunctionName} completed. {timer.FormatNextOccurrences(1)}");
        }

        private async Task PublishReleases()
        {
            var scheduled = (await QueryScheduledReleases()).Select(status => (status.ReleaseId, status.Id)).ToList();
            if (scheduled.Any())
            {
                foreach (var (releaseId, releaseStatusId) in scheduled)
                {
                    await _releaseStatusService.UpdateStateAsync(releaseId, releaseStatusId,
                        ScheduledReleaseStartedState);
                }

                await _queueService.QueuePublishReleaseFilesMessageAsync(scheduled);
                await _queueService.QueuePublishReleaseDataMessagesAsync(scheduled);
            }
        }

        private async Task<IEnumerable<ReleaseStatus>> QueryScheduledReleases()
        {
            var dateFilter = TableQuery.GenerateFilterConditionForDate(nameof(ReleaseStatus.Publish),
                QueryComparisons.LessThan, DateTime.Today.AddDays(1));
            var stageFilter = TableQuery.GenerateFilterCondition(nameof(ReleaseStatus.OverallStage),
                QueryComparisons.Equal,
                ReleaseStatusOverallStage.Scheduled.ToString());
            var combinedFilter = TableQuery.CombineFilters(dateFilter, TableOperators.And, stageFilter);
            var query = new TableQuery<ReleaseStatus>().Where(combinedFilter);

            return await _releaseStatusService.ExecuteQueryAsync(query);
        }
    }
}