using System;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
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
            _storageConnectionString = config.GetConnectionString("AzureStorage");
            _logger = logger;
        }

        public void PublishReleaseData(Guid releaseId)
        {
            var release = _context.Releases.FirstOrDefault(r => r.Id.Equals(releaseId));
            if (release == null)
            {
                throw new ArgumentException("Release does not exist", nameof(releaseId));
            }

            var storageAccount = CloudStorageAccount.Parse(_storageConnectionString);
            var client = storageAccount.CreateCloudQueueClient();
            var queue = client.GetQueueReference("publish-release-data");
            queue.CreateIfNotExists();

            var message = BuildMessage(releaseId);
            queue.AddMessage(message);

            _logger.LogTrace($"Sent publish release data message for release: {releaseId}");
        }
        
        private static CloudQueueMessage BuildMessage(Guid releaseId)
        {
            var message = new PublishReleaseDataMessage
            {
                ReleaseId = releaseId
            };
            
            return new CloudQueueMessage(JsonConvert.SerializeObject(message));
        }
    }
}