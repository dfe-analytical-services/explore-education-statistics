using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.Azure.Storage.Queue;
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

        private async Task ProcessReleases()
        {
            // TODO trigger this method based on time
            // TODO what is the storage connection string?
            var _storageConnectionString = "";

            var scheduledReleases = (await _releaseInfoService.GetScheduledReleasesAsync()).ToList();
            if (scheduledReleases.Any())
            {
                var publishReleaseDataFilesQueue =
                    await QueueUtils.GetQueueReferenceAsync(_storageConnectionString, "publish-release-data-files");
                var publishReleaseDataQueue =
                    await QueueUtils.GetQueueReferenceAsync(_storageConnectionString, "publish-release-data");
                var publishReleaseContentQueue =
                    await QueueUtils.GetQueueReferenceAsync(_storageConnectionString, "publish-release-content");

                scheduledReleases.ForEach(releaseInfo =>
                {
                    publishReleaseDataFilesQueue.AddMessage(
                        ToCloudQueueMessage(BuildPublishReleaseDataFilesMessage(releaseInfo)));
                    publishReleaseDataQueue.AddMessage(
                        ToCloudQueueMessage(BuildPublishReleaseDataMessage(releaseInfo)));
                    publishReleaseContentQueue.AddMessage(
                        ToCloudQueueMessage(BuildPublishReleaseContentMessage(releaseInfo)));
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

        private static PublishReleaseContentMessage BuildPublishReleaseContentMessage(ReleaseInfo releaseInfo)
        {
            return new PublishReleaseContentMessage
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