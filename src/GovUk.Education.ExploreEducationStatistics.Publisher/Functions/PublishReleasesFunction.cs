using System;
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

        private static readonly (Stage Content, Stage Files, Stage Data, Stage Overall) StartedStage =
            (Content: Queued, Files: Queued, Data: Queued, Overall: Started);

        public PublishReleasesFunction(IReleaseStatusService releaseStatusService)
        {
            _releaseStatusService = releaseStatusService;
        }

        /**
         * Azure function which publishes all Releases that are scheduled to be published during the day.
         */
        [FunctionName("PublishReleases")]
        public void PublishReleases([TimerTrigger("%PublishReleasesCronSchedule%")] TimerInfo timer,
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
            
            var dateQuery = TableQuery.GenerateFilterConditionForDate(nameof(ReleaseStatus.Publish), QueryComparisons.LessThan, DateTime.Today.AddDays(1));
            var statusQuery = TableQuery.GenerateFilterCondition(nameof(ReleaseStatus.Stage), QueryComparisons.Equal, Scheduled.ToString());
            var query = new TableQuery<ReleaseStatus>().Where(TableQuery.CombineFilters(dateQuery, TableOperators.And, statusQuery));

            var scheduled = (await _releaseStatusService.ExecuteQueryAsync(query)).ToList();
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
                    
                    await _releaseStatusService.UpdateStageAsync(releaseStatus.ReleaseId, releaseStatus.Id, StartedStage);
                }
            }
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