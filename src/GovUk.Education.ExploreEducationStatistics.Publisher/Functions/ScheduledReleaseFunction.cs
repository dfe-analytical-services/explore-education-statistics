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
        public void ScheduledRelease([TimerTrigger("0 */2 * * * *")] TimerInfo timer,
            ILogger logger)
        {
            logger.LogInformation($"{GetType().Name} function triggered at: {DateTime.Now}");
            ProcessReleases().Wait();
            logger.LogInformation(
                $"{GetType().Name} function completed. Next occurrence at: {timer.FormatNextOccurrences(1)}");
        }

        private async Task ProcessReleases()
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

                scheduledReleases.ForEach(releaseInfo =>
                {
                    generateReleaseContentQueue.AddMessage(
                        ToCloudQueueMessage(BuildGenerateReleaseContentMessage(releaseInfo)));
                    publishReleaseDataFilesQueue.AddMessage(
                        ToCloudQueueMessage(BuildPublishReleaseDataFilesMessage(releaseInfo)));
                    publishReleaseDataQueue.AddMessage(
                        ToCloudQueueMessage(BuildPublishReleaseDataMessage(releaseInfo)));
                    _releaseInfoService.UpdateReleaseInfoStatusAsync(releaseInfo.ReleaseId, releaseInfo.RowKey,
                        ReleaseInfoStatus.InProgress);
                });
            }
        }

        private static PublishReleaseDataFilesMessage BuildPublishReleaseDataFilesMessage(ReleaseInfo releaseInfo)
        {
            return new PublishReleaseDataFilesMessage
            {
                PublicationSlug = releaseInfo.PublicationSlug,
                ReleasePublished = releaseInfo.PublishScheduled,
                ReleaseSlug = releaseInfo.ReleaseSlug,
                ReleaseId = releaseInfo.ReleaseId
            };
        }

        private static PublishReleaseDataMessage BuildPublishReleaseDataMessage(ReleaseInfo releaseInfo)
        {
            return new PublishReleaseDataMessage
            {
                ReleaseId = releaseInfo.ReleaseId
            };
        }

        private static GenerateReleaseContentMessage BuildGenerateReleaseContentMessage(ReleaseInfo releaseInfo)
        {
            return new GenerateReleaseContentMessage
            {
                ReleaseId = releaseInfo.ReleaseId
            };
        }

        private static CloudQueueMessage ToCloudQueueMessage(object value)
        {
            return new CloudQueueMessage(JsonConvert.SerializeObject(value));
        }
    }
}