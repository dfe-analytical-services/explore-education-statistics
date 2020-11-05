using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Importer.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services
{
    public class FileImportService : IFileImportService
    {
        private readonly IBatchService _batchService;
        private readonly IFileStorageService _fileStorageService;
        private readonly IImporterService _importerService;
        private readonly ILogger<IFileImportService> _logger;
        private readonly IImportStatusService _importStatusService;

        public FileImportService(
            IFileStorageService fileStorageService,
            IImporterService importerService,
            IBatchService batchService,
            ILogger<IFileImportService> logger,
            IImportStatusService importStatusService)
        {
            _fileStorageService = fileStorageService;
            _importerService = importerService;
            _batchService = batchService;
            _logger = logger;
            _importStatusService = importStatusService;
        }

        public async Task ImportObservations(ImportMessage message, StatisticsDbContext context)
        {
            var releaseId = message.Release.Id;

            // Potentially status could already be failed so don't continue
            if (await _importStatusService.UpdateStatus(releaseId, message.OrigDataFileName, IStatus.STAGE_4))
            {
                var subjectData = await _fileStorageService.GetSubjectData(message);
                var releaseSubject = GetReleaseSubjectLink(message, context);

                await using var datafileStream = await _fileStorageService.StreamBlob(subjectData.DataBlob);
                var dataFileTable = DataTableUtils.CreateFromStream(datafileStream);

                await using var metaFileStream = await _fileStorageService.StreamBlob(subjectData.MetaBlob);
                var metaFileTable = DataTableUtils.CreateFromStream(metaFileStream);

                await context.Database.CreateExecutionStrategy().Execute(async () =>
                {
                    await using var transaction = await context.Database.BeginTransactionAsync();

                    await _importerService.ImportObservations(
                        dataFileTable.Columns,
                        dataFileTable.Rows,
                        releaseSubject.Subject,
                        _importerService.GetMeta(metaFileTable, releaseSubject.Subject, context),
                        message.BatchNo,
                        message.RowsPerBatch,
                        context
                    );

                    await transaction.CommitAsync();
                    await context.Database.CloseConnectionAsync();
                });

                await CheckComplete(releaseId, message, context);
            }
            else
            {
                _logger.LogInformation($"{message.DataFileName} already failed...skipping");
            }
        }

        public async Task ImportFiltersLocationsAndSchools(ImportMessage message, StatisticsDbContext context)
        {
            var subjectData = _fileStorageService.GetSubjectData(message).Result;
            var releaseSubject = GetReleaseSubjectLink(message, context);

            await using var dataFileStream = await _fileStorageService.StreamBlob(subjectData.DataBlob);
            var dataFileTable = DataTableUtils.CreateFromStream(dataFileStream);

            await using var metaFileStream = await _fileStorageService.StreamBlob(subjectData.MetaBlob);
            var metaFileTable = DataTableUtils.CreateFromStream(metaFileStream);

            await _importerService.ImportFiltersLocationsAndSchools(
                dataFileTable.Columns,
                dataFileTable.Rows,
                _importerService.GetMeta(metaFileTable, releaseSubject.Subject, context),
                context,
                message.Release.Id,
                message.OrigDataFileName);
        }

        private static ReleaseSubject GetReleaseSubjectLink(ImportMessage message, StatisticsDbContext context)
        {
            return context
                .ReleaseSubject
                .Include(r => r.Subject)
                .Include(r => r.Release)
                .ThenInclude(r => r.Publication)
                .ThenInclude(p => p.Topic)
                .ThenInclude(t => t.Theme)
                .FirstOrDefault(r => r.Subject.Id == message.SubjectId && r.ReleaseId == message.Release.Id);
        }
        
        private async Task CheckComplete(Guid releaseId, ImportMessage message, StatisticsDbContext context)
        {
            if (message.NumBatches > 1)
            {
                await _fileStorageService.DeleteBatchFile(releaseId.ToString(), message.DataFileName);
            }

            var numBatchesRemaining = await _fileStorageService.GetNumBatchesRemaining(releaseId.ToString(), message.OrigDataFileName);
            
            var import = await _importStatusService.GetImportStatus(releaseId, message.OrigDataFileName);

            if (message.NumBatches == 1 || numBatchesRemaining == 0)
            {
                var observationCount = context.Observation.Count(o => o.SubjectId.Equals(message.SubjectId));
                
                if (!observationCount.Equals(message.TotalRows))
                {
                    await _batchService.FailImport(releaseId, message.OrigDataFileName,
                        new List<ValidationError>
                        {
                            new ValidationError(
                                $"Number of observations inserted ({observationCount}) " +
                                $"does not equal that expected ({message.TotalRows}) : Please delete & retry"
                            )
                        }.AsEnumerable());
                }
                else
                {
                    await _importStatusService.UpdateStatus(releaseId, message.OrigDataFileName,
                        import.Errors.Equals("") ? IStatus.COMPLETE : IStatus.FAILED);
                }

                _logger.LogInformation(import.Errors.Equals("") && observationCount.Equals(message.TotalRows)
                    ? $"All batches imported for {releaseId} : {message.OrigDataFileName} with no errors"
                    : $"All batches imported for {releaseId} : {message.OrigDataFileName} but with errors - check storage log");
            }
            else
            {
                var percentageComplete = (double) (message.NumBatches - numBatchesRemaining) / message.NumBatches * 100;

                await _importStatusService.UpdateProgress(releaseId,
                    message.OrigDataFileName,
                    IStatus.STAGE_4,
                    percentageComplete);
            }
        }
    }
}