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
    public class NotificationsService : INotificationsService
    {
        private readonly ApplicationDbContext _context;
        private readonly string _storageConnectionString;

        private readonly ILogger _logger;

        public NotificationsService(ApplicationDbContext context,
            ILogger<NotificationsService> logger,
            IConfiguration config)
        {
            _context = context;
            _storageConnectionString = config.GetConnectionString("NotificationService");
            _logger = logger;
        }

        public bool NotifySubscribers(Guid publicationId)
        {
            if (CloudStorageAccount.TryParse(_storageConnectionString, out var storageAccount))
            {
                try
                {
                    var client = storageAccount.CreateCloudQueueClient();
                    var queue = client.GetQueueReference("publication-queue");
                    queue.CreateIfNotExists();

                    var message = BuildMessage(publicationId);
                    queue.AddMessage(message);
                }
                catch (StorageException ex)
                {
                    _logger.LogError("Error returned when adding notify subscribers message to queue: {0}", ex.Message);
                    return false;
                }
            }

            _logger.LogTrace($"Sent notify subscribers message for publication: {publicationId}");

            return true;
        }

        private CloudQueueMessage BuildMessage(Guid publicationId)
        {
            var message = _context.Publications
                .Where(p => p.Id.Equals(publicationId))
                .Select(p => new NotifySubscribersMessage
                {
                    Name = p.Title,
                    Slug = p.Slug,
                    PublicationId = p.Id.ToString()
                }).FirstOrDefault();

            return new CloudQueueMessage(JsonConvert.SerializeObject(message));
        }
    }
}