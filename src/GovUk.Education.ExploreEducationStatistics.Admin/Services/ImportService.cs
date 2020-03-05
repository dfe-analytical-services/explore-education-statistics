using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Storage.Queue;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using CloudStorageAccount = Microsoft.Azure.Storage.CloudStorageAccount;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class ImportService : IImportService
    {
        private readonly ContentDbContext _context;
        private readonly IMapper _mapper;
        private readonly string _storageConnectionString;
        private readonly ILogger _logger;
        private readonly CloudTable _table;

        public ImportService(ContentDbContext contentDbContext,
            IMapper mapper,
            ILogger<ImportService> logger,
            IConfiguration config,
            ITableStorageService tableStorageService)
        {
            _context = contentDbContext;
            _mapper = mapper;
            _storageConnectionString = config.GetValue<string>("CoreStorage");
            _logger = logger;
            _table = tableStorageService.GetTableAsync("imports").Result;
        }

        public async void Import(string dataFileName, Guid releaseId, IFormFile dataFile)
        {
            var storageAccount = CloudStorageAccount.Parse(_storageConnectionString);
            var client = storageAccount.CreateCloudQueueClient();
            var pQueue = client.GetQueueReference("imports-pending");
            var aQueue = client.GetQueueReference("imports-available");
            
            pQueue.CreateIfNotExists();
            aQueue.CreateIfNotExists();
            
            var message = BuildMessage(dataFileName, releaseId);
            
            await CreateImportTableRow(
                releaseId,
                dataFileName,
                FileStorageUtils.CalculateNumberOfRows(dataFile.OpenReadStream()),
                1,
                message);
            
            pQueue.AddMessage(new CloudQueueMessage(JsonConvert.SerializeObject(message)));

            _logger.LogInformation($"Sent import message for data file: {dataFileName}, releaseId: {releaseId}");
        }
        
        private async Task CreateImportTableRow(Guid releaseId, string dataFileName, int numberOfRows, int numBatches, ImportMessage message)
        {
            await _table.ExecuteAsync(TableOperation.InsertOrReplace(
                new DatafileImport(releaseId.ToString(), dataFileName, numberOfRows, numBatches, JsonConvert.SerializeObject(message)))
            );
        }

        private ImportMessage BuildMessage(string dataFileName, Guid releaseId)
        {
            var release = _context.Releases
                .Where(r => r.Id.Equals(releaseId))
                .Include(r => r.Publication)
                .ThenInclude(p => p.Topic)
                .ThenInclude(t => t.Theme)
                .FirstOrDefault();

            var importMessageRelease = _mapper.Map<Release>(release);
            
            return new ImportMessage
            {
                SubjectId = Guid.NewGuid(),
                DataFileName = dataFileName,
                OrigDataFileName = dataFileName,
                Release = importMessageRelease,
                BatchNo = 1
            };
        }
    }
}