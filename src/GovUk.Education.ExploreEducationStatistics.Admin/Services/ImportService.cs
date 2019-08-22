using System;
using System.Linq;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Queue;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class ImportService : IImportService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly string _storageConnectionString;
        
        private readonly ILogger _logger;

        public ImportService(ApplicationDbContext applicationDbContext,
            IMapper mapper,
            ILogger<ImportService> logger,
            IConfiguration config)
        {
            _context = applicationDbContext;
            _mapper = mapper;
            _storageConnectionString = config.GetConnectionString("CoreStorage");
            _logger = logger;
        }

        public void Import(string dataFileName, Guid releaseId)
        {
            var storageAccount = CloudStorageAccount.Parse(_storageConnectionString);
            var client = storageAccount.CreateCloudQueueClient();
            var pQueue = client.GetQueueReference("imports-pending");
            var aQueue = client.GetQueueReference("imports-available");
            
            pQueue.CreateIfNotExists();
            aQueue.CreateIfNotExists();
            
            var message = BuildMessage(dataFileName, releaseId);
            pQueue.AddMessage(message);

            _logger.LogInformation($"Sent import message for data file: {dataFileName}, releaseId: {releaseId}");
        }

        private CloudQueueMessage BuildMessage(string dataFileName, Guid releaseId)
        {
            var release = _context.Releases
                .Where(r => r.Id.Equals(releaseId))
                .Include(r => r.Publication)
                .ThenInclude(p => p.Topic)
                .ThenInclude(t => t.Theme)
                .FirstOrDefault();

            var importMessageRelease = _mapper.Map<Release>(release);
            var message = new ImportMessage
            {
                DataFileName = dataFileName,
                Release = importMessageRelease,
                BatchNo = 1,
                BatchSize = 1
            };

            return new CloudQueueMessage(JsonConvert.SerializeObject(message));
        }
    }
}