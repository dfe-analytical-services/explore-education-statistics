using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using Microsoft.Azure.Storage.Queue;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.QueueUtils;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Model.PublisherQueues;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class PublishingService : IPublishingService
    {
        private readonly ContentDbContext _context;
        private readonly string _storageConnectionString;

        private readonly ILogger _logger;

        public PublishingService(ContentDbContext contentDbContext,
            IConfiguration config,
            ILogger<PublishingService> logger
        )
        {
            _context = contentDbContext;
            _storageConnectionString = config.GetValue<string>("PublisherStorage");
            _logger = logger;
        }

        public async Task QueuePublishReleaseContentImmediateMessageAsync(Guid releaseId, Guid releaseStatusId)
        {
            var queue = await GetQueueReferenceAsync(_storageConnectionString, PublishReleaseContentImmediateQueue);
            await queue.AddMessageAsync(
                ToCloudQueueMessage(BuildPublishReleaseContentImmediateMessage(releaseId, releaseStatusId)));

            _logger.LogTrace($"Sent publish release content message for release: {releaseId}");
        }

        public async Task QueueValidateReleaseAsync(Guid releaseId, bool immediate = false)
        {
            var release = await GetRelease(releaseId);
            var queue = await GetQueueReferenceAsync(_storageConnectionString, ValidateReleaseQueue);
            await queue.AddMessageAsync(ToCloudQueueMessage(BuildValidateReleaseMessage(release, immediate)));

            _logger.LogTrace($"Sent release status message for release: {releaseId}");
        }

        private Task<Release> GetRelease(Guid releaseId)
        {
            return _context.Releases
                .AsNoTracking()
                .SingleAsync(release => release.Id == releaseId);
        }

        private static ValidateReleaseMessage BuildValidateReleaseMessage(Release release, bool immediate)
        {
            return new ValidateReleaseMessage
            {
                Immediate = immediate,
                ReleaseId = release.Id
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

        private static CloudQueueMessage ToCloudQueueMessage(object value)
        {
            return new CloudQueueMessage(JsonConvert.SerializeObject(value));
        }
    }
}