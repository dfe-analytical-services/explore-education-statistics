using System;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Functions;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.Azure.Storage.Queue;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Functions
{
    public class ScheduledReleaseFunction
    {
        private readonly IReleaseInfoService _releaseInfoService;

        public ScheduledReleaseFunction(IReleaseInfoService releaseInfoService)
        {
            _releaseInfoService = releaseInfoService;
        }

        [FunctionName("ScheduledRelease")]
        public void ScheduledRelease([TimerTrigger("0 0 0 * * *")] TimerInfo timer,
            ExecutionContext executionContext,
            ILogger logger)
        {
            logger.LogInformation($"{executionContext.FunctionName} triggered at: {DateTime.Now}");
            ProcessReleases(logger).Wait();
            logger.LogInformation(
                $"{executionContext.FunctionName} completed. {timer.FormatNextOccurrences(1)}");
        }

        private async Task ProcessReleases(ILogger logger)
        {
            var storageConnectionString = ConnectionUtils.GetAzureStorageConnectionString("PublisherStorage");

            var scheduledReleases = (await _releaseInfoService.GetScheduledReleasesAsync()).ToList();
            if (scheduledReleases.Any())
            {
                var generateReleaseContentQueue =
                    await QueueUtils.GetQueueReferenceAsync(storageConnectionString, "generate-release-content");
                var publishReleaseDataFilesQueue =
                    await QueueUtils.GetQueueReferenceAsync(storageConnectionString, "publish-release-data-files");
                var publishReleaseDataQueue =
                    await QueueUtils.GetQueueReferenceAsync(storageConnectionString, "publish-release-data");

                foreach (var releaseInfo in scheduledReleases)
                {
                    logger.LogInformation($"Adding messages for ReleaseInfo: {releaseInfo.ReleaseId}");
                    generateReleaseContentQueue.AddMessage(
                        ToCloudQueueMessage(BuildGenerateReleaseContentMessage(releaseInfo)));
                    publishReleaseDataFilesQueue.AddMessage(
                        ToCloudQueueMessage(BuildPublishReleaseDataFilesMessage(releaseInfo)));
                    publishReleaseDataQueue.AddMessage(
                        ToCloudQueueMessage(BuildPublishReleaseDataMessage(releaseInfo)));
                    await _releaseInfoService.UpdateReleaseInfoStatusAsync(releaseInfo.ReleaseId, releaseInfo.RowKey,
                        ReleaseInfoStatus.InProgress);
                }
            }
        }

        private static PublishReleaseDataFilesMessage BuildPublishReleaseDataFilesMessage(ReleaseInfo releaseInfo)
        {
            return new PublishReleaseDataFilesMessage
            {
                PublicationSlug = releaseInfo.PublicationSlug,
                ReleasePublished = releaseInfo.PublishScheduled,
                ReleaseSlug = releaseInfo.ReleaseSlug,
                ReleaseId = releaseInfo.ReleaseId,
                ReleaseInfoId = Guid.Parse(releaseInfo.RowKey)
            };
        }

        private static PublishReleaseDataMessage BuildPublishReleaseDataMessage(ReleaseInfo releaseInfo)
        {
            return new PublishReleaseDataMessage
            {
                ReleaseId = releaseInfo.ReleaseId,
                ReleaseInfoId = Guid.Parse(releaseInfo.RowKey)
            };
        }

        private static GenerateReleaseContentMessage BuildGenerateReleaseContentMessage(ReleaseInfo releaseInfo)
        {
            return new GenerateReleaseContentMessage
            {
                ReleaseId = releaseInfo.ReleaseId,
                ReleaseInfoId = Guid.Parse(releaseInfo.RowKey)
            };
        }

        private static CloudQueueMessage ToCloudQueueMessage(object value)
        {
            return new CloudQueueMessage(JsonConvert.SerializeObject(value));
        }
    }
}