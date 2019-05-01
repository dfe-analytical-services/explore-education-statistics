using GovUk.Education.ExploreStatistics.Admin.Models;
using GovUk.Education.ExploreStatistics.Admin.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreStatistics.Admin.Services
{
    public class ImportService : IImportService
    {
        private readonly string _storageConnectionString;

        private readonly ILogger _logger;

        public ImportService(ILogger<ImportService> logger, IConfiguration config)
        {
            _storageConnectionString = config.GetConnectionString("NotificationService");
            _logger = logger;
        }

        public bool SendImportNotification(ImportViewModel model)
        {
            if (CloudStorageAccount.TryParse(_storageConnectionString, out var storageAccount))
            {
                try
                {
                    var client = storageAccount.CreateCloudQueueClient();

                    var queue = client.GetQueueReference("imports-pending");
                    queue.CreateIfNotExists();
                    
                    var body = JsonConvert.SerializeObject(model);
                    
                    var message = new CloudQueueMessage(body);
                    queue.AddMessage(message);

                }
                catch (StorageException ex)
                {
                    _logger.LogError("Error returned when adding import notification to queue: {0}", ex.Message);
                    return false;
                }
            }
            
            _logger.LogTrace("Sent files import notification for {0} ({1})", model.PublicationName, model.PublicationId);

            return true;
        }
    }
}