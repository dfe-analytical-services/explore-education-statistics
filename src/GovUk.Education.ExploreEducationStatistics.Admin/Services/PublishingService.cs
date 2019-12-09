using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using Microsoft.Azure.Storage.Queue;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

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
            _storageConnectionString = config.GetConnectionString("PublicStorage");
            _logger = logger;
        }

        public async Task<QueueReleaseMessage> QueueReleaseAsync(Guid releaseId)
        {
            var release = await GetRelease(releaseId);

            if (release == null)
            {
                throw new ArgumentException("Release does not exist", nameof(releaseId));
            }

            var queue = await QueueUtils.GetQueueReferenceAsync(_storageConnectionString, "releases");
            var message = BuildQueueReleaseMessage(release);
            queue.AddMessage(ToCloudQueueMessage(message));

            _logger.LogTrace($"Sent queue release message for release: {releaseId}");

            return message;
        }

        private ValueTask<Release> GetRelease(Guid releaseId)
        {
            return _context.Releases.FindAsync(releaseId);
        }

        private static QueueReleaseMessage BuildQueueReleaseMessage(Release release)
        {
            return new QueueReleaseMessage
            {
                ReleaseId = release.Id,
                PublishScheduled = release.PublishScheduled.GetValueOrDefault()
            };
        }

        private static CloudQueueMessage ToCloudQueueMessage(object value)
        {
            return new CloudQueueMessage(JsonConvert.SerializeObject(value));
        }
    }
}