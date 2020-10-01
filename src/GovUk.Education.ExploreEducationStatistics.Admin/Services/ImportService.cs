using System;
using System.Collections.Generic;
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
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Storage.Queue;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;
using CloudStorageAccount = Microsoft.Azure.Storage.CloudStorageAccount;
using static GovUk.Education.ExploreEducationStatistics.Common.TableStorageTableNames;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class ImportService : IImportService
    {
        private readonly ContentDbContext _context;
        private readonly IMapper _mapper;
        private readonly string _storageConnectionString;
        private readonly ILogger _logger;
        private readonly IGuidGenerator _guidGenerator;
        private readonly ITableStorageService _tableStorageService;

        public ImportService(ContentDbContext contentDbContext,
            IMapper mapper,
            ILogger<ImportService> logger,
            IConfiguration config,
            ITableStorageService tableStorageService,
            IGuidGenerator guidGenerator)
        {
            _context = contentDbContext;
            _mapper = mapper;
            _storageConnectionString = config.GetValue<string>("CoreStorage");
            _logger = logger;
            _tableStorageService = tableStorageService;
            _guidGenerator = guidGenerator;
        }

        public async Task Import(Guid releaseId, string dataFileName, string metaFileName, IFormFile dataFile, bool isZip)
        {
            var storageAccount = CloudStorageAccount.Parse(_storageConnectionString);
            var client = storageAccount.CreateCloudQueueClient();
            var pQueue = client.GetQueueReference("imports-pending");
            var aQueue = client.GetQueueReference("imports-available");

            pQueue.CreateIfNotExists();
            aQueue.CreateIfNotExists();
            // TODO - EES-1250
            var numRows = isZip ? 0 : FileStorageUtils.CalculateNumberOfRows(dataFile.OpenReadStream());
            var message = BuildMessage(dataFileName, metaFileName, releaseId, isZip ? dataFile.FileName.ToLower() : "");

            await UpdateImportTableRow(
                releaseId,
                dataFileName,
                numRows,
                message);

            pQueue.AddMessage(new CloudQueueMessage(JsonConvert.SerializeObject(message)));

            _logger.LogInformation($"Sent import message for data file: {dataFileName}, releaseId: {releaseId}");
        }

        public async Task<Either<ActionResult, Unit>> CreateImportTableRow(Guid releaseId, string dataFileName)
        {
            var result = await _tableStorageService.RetrieveEntity(DatafileImportsTableName,
                new DatafileImport(releaseId.ToString(), dataFileName), new List<string>());

            if (result.Result != null)
            {
                return ValidationActionResult(DataFileAlreadyUploaded);
            }

            await _tableStorageService.CreateOrUpdateEntity(DatafileImportsTableName,
                new DatafileImport(releaseId.ToString(), dataFileName));

            return Unit.Instance;
        }

        public async Task RemoveImportTableRowIfExists(Guid releaseId, string dataFileName)
        {
            await _tableStorageService.DeleteEntityAsync(DatafileImportsTableName,
                new DatafileImport(releaseId.ToString(), dataFileName));
        }

        public async Task FailImport(Guid releaseId, string dataFileName, IEnumerable<ValidationError> errors)
        {
            await _tableStorageService.CreateOrUpdateEntity(DatafileImportsTableName,
                new DatafileImport(releaseId.ToString(), dataFileName, 0, "", IStatus.FAILED, JsonConvert.SerializeObject(errors)));
        }

        private async Task UpdateImportTableRow(Guid releaseId, string dataFileName, int numberOfRows, ImportMessage message)
        {
            await _tableStorageService.CreateOrUpdateEntity(DatafileImportsTableName,
                new DatafileImport(releaseId.ToString(), dataFileName, numberOfRows,
                    JsonConvert.SerializeObject(message), IStatus.QUEUED));
        }

        private ImportMessage BuildMessage(string dataFileName, string metaFileName, Guid releaseId, string zipFileName)
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
                SubjectId = _guidGenerator.NewGuid(),
                DataFileName = dataFileName,
                OrigDataFileName = dataFileName,
                MetaFileName = metaFileName,
                Release = importMessageRelease,
                NumBatches = 1,
                BatchNo = 1,
                ArchiveFileName = zipFileName
            };
        }
    }

    public class ValidationError
    {
        public string Message { get; set; }

        public ValidationError(string message)
        {
            Message = message;
        }
    }
}