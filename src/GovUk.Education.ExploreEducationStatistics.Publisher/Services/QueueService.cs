using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Model.PublisherQueues;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services
{
    public class QueueService : IQueueService
    {
        private readonly IStorageQueueService _storageQueueService;
        private readonly IReleasePublishingStatusService _releasePublishingStatusService;
        private readonly ILogger<QueueService> _logger;

        public QueueService(IStorageQueueService storageQueueService,
            IReleasePublishingStatusService releasePublishingStatusService,
            ILogger<QueueService> logger)
        {
            _storageQueueService = storageQueueService;
            _releasePublishingStatusService = releasePublishingStatusService;
            _logger = logger;
        }

        public async Task QueueGenerateReleaseContentMessageAsync(
            IEnumerable<(Guid ReleaseId, Guid ReleaseStatusId)> releases)
        {
            var releasesList = releases.ToList();
            _logger.LogInformation(
                "Queuing generate content message for releases: {0}",
                string.Join(", ", releasesList.Select(tuple => tuple.ReleaseId)));
            await _storageQueueService.AddMessageAsync(
                GenerateReleaseContentQueue, new GenerateReleaseContentMessage(releasesList));
            foreach (var (releaseId, releaseStatusId) in releasesList)
            {
                await _releasePublishingStatusService.UpdateContentStageAsync(releaseId, releaseStatusId,
                    ReleasePublishingStatusContentStage.Queued);
            }
        }

        public async Task QueuePublishReleaseContentMessageAsync(Guid releaseId, Guid releaseStatusId)
        {
            _logger.LogInformation("Queuing publish content message for release: {0}", releaseId);
            await _storageQueueService.AddMessageAsync(
                PublishReleaseContentQueue, new PublishReleaseContentMessage(releaseId, releaseStatusId));
            await _releasePublishingStatusService.UpdateContentStageAsync(releaseId, releaseStatusId,
                ReleasePublishingStatusContentStage.Queued);
        }

        public Task QueuePublishReleaseDataMessageAsync(Guid releaseId, Guid releaseStatusId)
        {
            return QueuePublishReleaseDataMessagesAsync(new[] {(releaseId, releaseStatusId)});
        }

        public async Task QueuePublishReleaseDataMessagesAsync(
            IEnumerable<(Guid ReleaseId, Guid ReleaseStatusId)> releases)
        {
            foreach (var (releaseId, releaseStatusId) in releases)
            {
                _logger.LogInformation("Queuing data message for release: {0}", releaseId);
                await _storageQueueService.AddMessageAsync(
                    PublishReleaseDataQueue, new PublishReleaseDataMessage(releaseId, releaseStatusId));
                await _releasePublishingStatusService.UpdateDataStageAsync(releaseId, releaseStatusId,
                    ReleasePublishingStatusDataStage.Queued);
            }
        }

        public Task QueuePublishReleaseFilesMessageAsync(Guid releaseId, Guid releaseStatusId)
        {
            return QueuePublishReleaseFilesMessageAsync(new[] {(releaseId, releaseStatusId)});
        }

        public async Task QueuePublishReleaseFilesMessageAsync(
            IEnumerable<(Guid ReleaseId, Guid ReleaseStatusId)> releases)
        {
            var releasesList = releases.ToList();
            _logger.LogInformation(
                "Queuing files message for releases: {0}",
                string.Join(", ", releasesList.Select(tuple => tuple.ReleaseId)));
            await _storageQueueService.AddMessageAsync(
                PublishReleaseFilesQueue, new PublishReleaseFilesMessage(releasesList));
            foreach (var (releaseId, releaseStatusId) in releasesList)
            {
                await _releasePublishingStatusService.UpdateFilesStageAsync(releaseId, releaseStatusId,
                    ReleasePublishingStatusFilesStage.Queued);
            }
        }
    }
}
