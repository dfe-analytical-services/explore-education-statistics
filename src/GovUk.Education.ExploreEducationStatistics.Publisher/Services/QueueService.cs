using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Functions;
using GovUk.Education.ExploreEducationStatistics.Publisher.Functions;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.Azure.Storage.Queue;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.QueueUtils;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services
{
    public class QueueService : IQueueService
    {
        private readonly string _storageConnectionString =
            ConnectionUtils.GetAzureStorageConnectionString("PublisherStorage");

        private readonly ILogger<QueueService> _logger;

        public QueueService(ILogger<QueueService> logger)
        {
            _logger = logger;
        }

        public async Task QueueGenerateReleaseContentMessageAsync(Guid releaseId, Guid releaseStatusId)
        {
            var queue = await GetQueueReferenceAsync(_storageConnectionString,
                GenerateReleaseContentFunction.QueueName);
            _logger.LogInformation($"Queuing content message for release: {releaseId}");
            await queue.AddMessageAsync(
                ToCloudQueueMessage(BuildGenerateReleaseContentMessage(releaseId, releaseStatusId)));
        }

        public async Task QueuePublishReleaseDataMessagesAsync(
            IEnumerable<(Guid ReleaseId, Guid ReleaseStatusId)> releases)
        {
            var queue = await GetQueueReferenceAsync(_storageConnectionString, PublishReleaseDataFunction.QueueName);
            foreach (var (releaseId, releaseStatusId) in releases)
            {
                _logger.LogInformation($"Queuing data message for release: {releaseId}");
                await queue.AddMessageAsync(
                    ToCloudQueueMessage(BuildPublishReleaseDataMessage(releaseId, releaseStatusId)));
            }
        }

        public async Task QueuePublishReleaseFilesMessagesAsync(
            IEnumerable<(Guid ReleaseId, Guid ReleaseStatusId)> releases)
        {
            var queue = await GetQueueReferenceAsync(_storageConnectionString, PublishReleaseFilesFunction.QueueName);
            foreach (var (releaseId, releaseStatusId) in releases)
            {
                _logger.LogInformation($"Queuing files message for release: {releaseId}");
                await queue.AddMessageAsync(
                    ToCloudQueueMessage(BuildPublishReleaseFilesMessage(releaseId, releaseStatusId)));
            }
        }

        private static PublishReleaseFilesMessage BuildPublishReleaseFilesMessage(Guid releaseId, Guid releaseStatusId)
        {
            return new PublishReleaseFilesMessage
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

        private static GenerateReleaseContentMessage BuildGenerateReleaseContentMessage(Guid releaseId,
            Guid releaseStatusId)
        {
            return new GenerateReleaseContentMessage
            {
                ReleaseId = releaseId,
                ReleaseStatusId = releaseStatusId
            };
        }

        private static CloudQueueMessage ToCloudQueueMessage(object value)
        {
            return new CloudQueueMessage(JsonConvert.SerializeObject(value));
        }
    }
}