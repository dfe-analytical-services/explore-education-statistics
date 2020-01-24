using System;
using System.Collections.Generic;
using System.Linq;
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

        public async Task QueueGenerateReleaseContentMessageAsync(
            IEnumerable<(Guid ReleaseId, Guid ReleaseStatusId)> releases)
        {
            var queue = await GetQueueReferenceAsync(_storageConnectionString,
                GenerateReleaseContentFunction.QueueName);
            var releasesList = releases.ToList();
            _logger.LogInformation(
                $"Queuing content message for releases: {releasesList.Select(tuple => tuple.ReleaseId)}");
            await queue.AddMessageAsync(ToCloudQueueMessage(BuildGenerateReleaseContentMessage(releasesList)));
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

        public async Task QueuePublishReleaseFilesMessageAsync(
            IEnumerable<(Guid ReleaseId, Guid ReleaseStatusId)> releases)
        {
            var queue = await GetQueueReferenceAsync(_storageConnectionString, PublishReleaseFilesFunction.QueueName);
            var releasesList = releases.ToList();
            _logger.LogInformation(
                $"Queuing files message for releases: {releasesList.Select(tuple => tuple.ReleaseId)}");
            await queue.AddMessageAsync(ToCloudQueueMessage(BuildPublishReleaseFilesMessage(releasesList)));
        }

        private static PublishReleaseFilesMessage BuildPublishReleaseFilesMessage(
            IEnumerable<(Guid ReleaseId, Guid ReleaseStatusId)> releases)
        {
            return new PublishReleaseFilesMessage
            {
                Releases = releases
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

        private static GenerateReleaseContentMessage BuildGenerateReleaseContentMessage(
            IEnumerable<(Guid ReleaseId, Guid ReleaseStatusId)> releases)
        {
            return new GenerateReleaseContentMessage
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