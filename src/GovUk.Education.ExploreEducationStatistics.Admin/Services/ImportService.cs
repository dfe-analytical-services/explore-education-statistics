using System;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
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

        public void SendImportNotification(Guid releaseId, string dataFileName, string metaFileName)
        {
            var storageAccount = CloudStorageAccount.Parse(_storageConnectionString);
            var client = storageAccount.CreateCloudQueueClient();
            var queue = client.GetQueueReference("imports-pending");
            queue.CreateIfNotExists();

            var content = JsonConvert.SerializeObject(new ImportMessagePayload
            {
                ReleaseId = releaseId,
                DataFileName = dataFileName,
                MetaFileName = metaFileName
            });

            var message = new CloudQueueMessage(content);
            queue.AddMessage(message);

            _logger.LogTrace("Sent import notification: {0}", content);
        }
    }
}