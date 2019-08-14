using System;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class PublishingService : IPublishingService
    {
        private readonly ApplicationDbContext _context;
        private readonly string _storageConnectionString;

        private readonly ILogger _logger;

        public PublishingService(ApplicationDbContext applicationDbContext,
            IConfiguration config,
            ILogger<ImportService> logger
        )
        {
            _context = applicationDbContext;
            _storageConnectionString = config.GetConnectionString("PublicStorage");
            _logger = logger;
        }

        public void PublishReleaseData(Guid releaseId)
        {
            var release = _context.Releases
                .Where(r => r.Id.Equals(releaseId))
                .Include(r => r.Publication)
                .FirstOrDefault();
            
            if (release == null)
            {
                throw new ArgumentException("Release does not exist", nameof(releaseId));
            }

            var storageAccount = CloudStorageAccount.Parse(_storageConnectionString);
            var client = storageAccount.CreateCloudQueueClient();
            var queue = client.GetQueueReference("publish-release-data");
            queue.CreateIfNotExists();

            var message = BuildMessage(release);
            queue.AddMessage(message);

            _logger.LogTrace($"Sent publish release data message for release: {releaseId}");
        }

        private static CloudQueueMessage BuildMessage(Release release)
        {
            var message = new PublishReleaseDataMessage
            {
                PublicationSlug = release.Publication.Slug,
                ReleasePublished = release.Published ?? DateTime.UtcNow,
                ReleaseSlug = release.Slug,
                ReleaseId = release.Id
            };

            return new CloudQueueMessage(JsonConvert.SerializeObject(message));
        }
    }
}