using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Functions;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Storage.Queue;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Model.Stage;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Functions
{
    public class PublishReleasesFunction
    {
        private readonly IReleaseStatusService _releaseStatusService;

        private static readonly ReleaseStatusState StartedState = new ReleaseStatusState(Queued, Queued, Queued, Scheduled, Started);

        public PublishReleasesFunction(IReleaseStatusService releaseStatusService)
        {
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
            PublishReleases(logger).Wait();
            logger.LogInformation(
                $"{executionContext.FunctionName} completed. {timer.FormatNextOccurrences(1)}");
        }

        private async Task PublishReleases(ILogger logger)
        {
            var storageConnectionString = ConnectionUtils.GetAzureStorageConnectionString("PublisherStorage");
            var scheduled = (await QueryScheduledReleases()).ToList();
            if (scheduled.Any())
            {
                var generateReleaseContentQueue =
                    await QueueUtils.GetQueueReferenceAsync(storageConnectionString,
                        GenerateReleaseContentFunction.QueueName);
                var publishReleaseFilesQueue =
                    await QueueUtils.GetQueueReferenceAsync(storageConnectionString,
                        PublishReleaseFilesFunction.QueueName);
                var publishReleaseDataQueue =
                    await QueueUtils.GetQueueReferenceAsync(storageConnectionString,
                        PublishReleaseDataFunction.QueueName);

                foreach (var releaseStatus in scheduled)
                {
                    logger.LogInformation($"Adding messages for release: {releaseStatus.ReleaseId}");
                    generateReleaseContentQueue.AddMessage(
                        ToCloudQueueMessage(BuildGenerateReleaseContentMessage(releaseStatus)));
                    publishReleaseFilesQueue.AddMessage(
                        ToCloudQueueMessage(BuildPublishReleaseFilesMessage(releaseStatus)));
                    publishReleaseDataQueue.AddMessage(
                        ToCloudQueueMessage(BuildPublishReleaseDataMessage(releaseStatus)));

                    await _releaseStatusService.UpdateStateAsync(releaseStatus.ReleaseId, releaseStatus.Id,
                        StartedState);
                }
            }
        }

        private async Task<IEnumerable<ReleaseStatus>> QueryScheduledReleases()
        {
            var dateQuery = TableQuery.GenerateFilterConditionForDate(nameof(ReleaseStatus.Publish),
                QueryComparisons.LessThan, DateTime.Today.AddDays(1));
            var stageQuery = TableQuery.GenerateFilterCondition(nameof(ReleaseStatus.OverallStage), QueryComparisons.Equal,
                Scheduled.ToString());
            var query = new TableQuery<ReleaseStatus>().Where(TableQuery.CombineFilters(dateQuery, TableOperators.And,
                stageQuery));

            return await _releaseStatusService.ExecuteQueryAsync(query);
        }

        private static PublishReleaseFilesMessage BuildPublishReleaseFilesMessage(ReleaseStatus releaseStatus)
        {
            return new PublishReleaseFilesMessage
            {
                PublicationSlug = releaseStatus.PublicationSlug,
                ReleasePublished = releaseStatus.Publish.Value,
                ReleaseSlug = releaseStatus.ReleaseSlug,
                ReleaseId = releaseStatus.ReleaseId,
                ReleaseStatusId = releaseStatus.Id
            };
        }

        private static PublishReleaseDataMessage BuildPublishReleaseDataMessage(ReleaseStatus releaseStatus)
        {
            return new PublishReleaseDataMessage
            {
                ReleaseId = releaseStatus.ReleaseId,
                ReleaseStatusId = releaseStatus.Id
            };
        }

        private static GenerateReleaseContentMessage BuildGenerateReleaseContentMessage(ReleaseStatus releaseStatus)
        {
            return new GenerateReleaseContentMessage
            {
                ReleaseId = releaseStatus.ReleaseId,
                ReleaseStatusId = releaseStatus.Id
            };
        }

        private static CloudQueueMessage ToCloudQueueMessage(object value)
        {
            return new CloudQueueMessage(JsonConvert.SerializeObject(value));
        }
    }
}