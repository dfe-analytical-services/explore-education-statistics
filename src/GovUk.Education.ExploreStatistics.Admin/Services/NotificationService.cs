using GovUk.Education.ExploreStatistics.Admin.Models;
using GovUk.Education.ExploreStatistics.Admin.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace GovUk.Education.ExploreStatistics.Admin.Services
{
    public class NotificationService : INotificationService
    {
        private readonly string _storageConnectionString;

        private readonly ILogger _logger;

        public NotificationService(ILogger<NotificationService> logger, IConfiguration config)
        {
            _storageConnectionString = config.GetConnectionString("NotificationService");
            _logger = logger;
        }

        public bool SendNotification(PublicationViewModel publication)
        {
            if (CloudStorageAccount.TryParse(_storageConnectionString, out var storageAccount))
            {
                try
                {
                    var client = storageAccount.CreateCloudQueueClient();

                    var queue = client.GetQueueReference("publication-queue");
                    queue.CreateIfNotExists();
                    
                    var body = JsonConvert.SerializeObject(publication);
                    
                    var message = new CloudQueueMessage(body);
                    queue.AddMessage(message);

                }
                catch (StorageException ex)
                {
                    _logger.LogError("Error returned when adding release notification to queue: {0}", ex.Message);
                    return false;
                }
            }
            
            _logger.LogTrace("Sent notification for {0} ({1})", publication.Name, publication.PublicationId);

            return true;
        }
    }
}