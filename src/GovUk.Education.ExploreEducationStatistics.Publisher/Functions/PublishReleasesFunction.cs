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
    public class PublishReleasesFunction
    {
        private readonly IQueueService _queueService;
        private readonly IReleaseStatusService _releaseStatusService;
        
        private static readonly ReleaseStatusState StartedState =
            new ReleaseStatusState(NotStarted, Queued, Queued, Scheduled, Started);

        public PublishReleasesFunction(IQueueService queueService, IReleaseStatusService releaseStatusService)
        {
            _queueService = queueService;
            _releaseStatusService = releaseStatusService;
        }

        /**
         * Azure function which publishes all Releases that are scheduled to be published during the day.
         */
        [FunctionName("PublishReleases")]
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
                await _queueService.QueuePublishReleaseFilesMessageAsync(scheduled);
                await _queueService.QueuePublishReleaseDataMessagesAsync(scheduled);
                foreach (var (releaseId, releaseStatusId) in scheduled)
                {
                    await _releaseStatusService.UpdateStateAsync(releaseId, releaseStatusId, StartedState);
                }
            }
        }

        private async Task<IEnumerable<ReleaseStatus>> QueryScheduledReleases()
        {
            var dateQuery = TableQuery.GenerateFilterConditionForDate(nameof(ReleaseStatus.Publish),
                QueryComparisons.LessThan, DateTime.Today.AddDays(1));
            var stageQuery = TableQuery.GenerateFilterCondition(nameof(ReleaseStatus.OverallStage),
                QueryComparisons.Equal,
                Scheduled.ToString());
            var query = new TableQuery<ReleaseStatus>().Where(TableQuery.CombineFilters(dateQuery, TableOperators.And,
                stageQuery));

            return await _releaseStatusService.ExecuteQueryAsync(query);
        }
    }
}