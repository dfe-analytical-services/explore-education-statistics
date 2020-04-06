using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Publisher.Functions;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.Azure.Storage.Queue;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.QueueUtils;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Model.PublisherQueues;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services
{
    public class QueueService : IQueueService
    {
        private readonly string _storageConnectionString;
        private readonly IReleaseStatusService _releaseStatusService;
        private readonly ILogger<QueueService> _logger;

        public QueueService(IConfiguration configuration,
            IReleaseStatusService releaseStatusService,
            ILogger<QueueService> logger)
        {
            _storageConnectionString = configuration.GetValue<string>("PublisherStorage");
            _releaseStatusService = releaseStatusService;
            _logger = logger;
        }

        public async Task QueueGenerateReleaseContentMessageAsync(
            IEnumerable<(Guid ReleaseId, Guid ReleaseStatusId)> releases)
        {
            var queue = await GetQueueReferenceAsync(_storageConnectionString, GenerateReleaseContentQueue);
            var releasesList = releases.ToList();
            _logger.LogInformation(
                $"Queuing generate content message for releases: {string.Join(", ", releasesList.Select(tuple => tuple.ReleaseId))}");
            await queue.AddMessageAsync(ToCloudQueueMessage(BuildGenerateReleaseContentMessage(releasesList)));
            foreach (var (releaseId, releaseStatusId) in releasesList)
            {
                await _releaseStatusService.UpdateContentStageAsync(releaseId, releaseStatusId,
                    ReleaseStatusContentStage.Queued);
            }
        }

        public async Task QueuePublishReleaseContentImmediateMessageAsync(Guid releaseId, Guid releaseStatusId)
        {
            var queue = await GetQueueReferenceAsync(_storageConnectionString, PublishReleaseContentImmediateQueue);
            _logger.LogInformation($"Queuing publish content message for release: {releaseId}");
            await queue.AddMessageAsync(
                ToCloudQueueMessage(BuildPublishReleaseContentImmediateMessage(releaseId, releaseStatusId)));
            await _releaseStatusService.UpdateContentStageAsync(releaseId, releaseStatusId,
                ReleaseStatusContentStage.Queued);
        }

        public async Task QueuePublishReleaseDataMessagesAsync(
            IEnumerable<(Guid ReleaseId, Guid ReleaseStatusId)> releases)
        {
            var queue = await GetQueueReferenceAsync(_storageConnectionString, PublishReleaseDataQueue);
            foreach (var (releaseId, releaseStatusId) in releases)
            {
                _logger.LogInformation($"Queuing data message for release: {releaseId}");
                await queue.AddMessageAsync(
                    ToCloudQueueMessage(BuildPublishReleaseDataMessage(releaseId, releaseStatusId)));
                await _releaseStatusService.UpdateDataStageAsync(releaseId, releaseStatusId,
                    ReleaseStatusDataStage.Queued);
            }
        }

        public Task QueuePublishReleaseFilesMessageAsync(Guid releaseId, Guid releaseStatusId)
        {
            return QueuePublishReleaseFilesMessageAsync(new[] {(releaseId, releaseStatusId)});
        }

        public async Task QueuePublishReleaseFilesMessageAsync(
            IEnumerable<(Guid ReleaseId, Guid ReleaseStatusId)> releases)
        {
            var queue = await GetQueueReferenceAsync(_storageConnectionString, PublishReleaseFilesQueue);
            var releasesList = releases.ToList();
            _logger.LogInformation(
                $"Queuing files message for releases: {releasesList.Select(tuple => tuple.ReleaseId)}");
            await queue.AddMessageAsync(ToCloudQueueMessage(BuildPublishReleaseFilesMessage(releasesList)));
            foreach (var (releaseId, releaseStatusId) in releasesList)
            {
                await _releaseStatusService.UpdateFilesStageAsync(releaseId, releaseStatusId,
                    ReleaseStatusFilesStage.Queued);
            }
        }

        private static GenerateReleaseContentMessage BuildGenerateReleaseContentMessage(
            IEnumerable<(Guid ReleaseId, Guid ReleaseStatusId)> releases)
        {
            return new GenerateReleaseContentMessage
            {
                Releases = releases
            };
        }

        private static PublishReleaseContentImmediateMessage BuildPublishReleaseContentImmediateMessage(
            Guid releaseId, Guid releaseStatusId)
        {
            return new PublishReleaseContentImmediateMessage
            {
                ReleaseId = releaseId,
                ReleaseStatusId = releaseStatusId
            };
        }

        private static PublishReleaseDataMessage BuildPublishReleaseDataMessage(Guid releaseId, Guid releaseStatusId)
        {
            return new PublishReleaseDataMessage
            {
                ReleaseId = releaseId,
                ReleaseStatusId = releaseStatusId
            };
        }

        private static PublishReleaseFilesMessage BuildPublishReleaseFilesMessage(
            IEnumerable<(Guid ReleaseId, Guid ReleaseStatusId)> releases)
        {
            return new PublishReleaseFilesMessage
            {
                Releases = releases
            };
        }

        private static CloudQueueMessage ToCloudQueueMessage(object value)
        {
            return new CloudQueueMessage(JsonConvert.SerializeObject(value));
        }
    }
}