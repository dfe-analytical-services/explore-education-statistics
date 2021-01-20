using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.TableStorageTableNames;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class ImportService : IImportService
    {
        private readonly ContentDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;
        private readonly IStorageQueueService _queueService;
        private readonly ITableStorageService _tableStorageService;
        private readonly IUserService _userService;

        public ImportService(ContentDbContext contentDbContext,
            IMapper mapper,
            ILogger<ImportService> logger,
            IStorageQueueService queueService,
            ITableStorageService tableStorageService,
            IUserService userService)
        {
            _context = contentDbContext;
            _mapper = mapper;
            _logger = logger;
            _queueService = queueService;
            _tableStorageService = tableStorageService;
            _userService = userService;
        }

        public async Task Import(Guid releaseId,
            Guid subjectId,
            string dataFileName,
            string metaFileName,
            IFormFile dataFile,
            bool isZip)
        {
            // TODO - EES-1250
            var numRows = isZip ? 0 : FileStorageUtils.CalculateNumberOfRows(dataFile.OpenReadStream());
            var message = BuildMessage(subjectId, dataFileName, metaFileName, releaseId, isZip ? dataFile.FileName.ToLower() : "");

            await UpdateImportTableRow(
                releaseId,
                dataFileName,
                numRows,
                message);

            await _queueService.AddMessageAsync("imports-pending", message);

            _logger.LogInformation($"Sent import message for data file: {dataFileName}, releaseId: {releaseId}");
        }

        public Task<Either<ActionResult, Unit>> CancelImport(ReleaseFileImportInfo import)
        {
            return _userService
                .CheckCanCancelFileImport(import)
                .OnSuccessVoid(async () =>
                {
                    await _queueService.AddMessageAsync("imports-cancelling", new CancelImportMessage
                    {
                        ReleaseId = import.ReleaseId,
                        DataFileName = import.DataFileName
                    });
                });
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

        public async Task FailImport(Guid releaseId, Guid subjectId, string dataFileName, string metaFileName,
            IEnumerable<ValidationError> errors)
        {
            var importReleaseMessage =
                JsonConvert.SerializeObject(BuildMessage(subjectId, dataFileName, metaFileName, releaseId));

            await _tableStorageService.CreateOrUpdateEntity(DatafileImportsTableName,
                new DatafileImport(releaseId.ToString(), dataFileName, 0, importReleaseMessage,
                    IStatus.FAILED, JsonConvert.SerializeObject(errors)));
        }

        private async Task UpdateImportTableRow(Guid releaseId, string dataFileName, int numberOfRows, ImportMessage message)
        {
            await _tableStorageService.CreateOrUpdateEntity(DatafileImportsTableName,
                new DatafileImport(releaseId.ToString(), dataFileName, numberOfRows,
                    JsonConvert.SerializeObject(message), IStatus.QUEUED));
        }

        private ImportMessage BuildMessage(Guid subjectId, 
            string dataFileName,
            string metaFileName,
            Guid releaseId,
            string zipFileName = "")
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
                SubjectId = subjectId,
                DataFileName = dataFileName,
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
