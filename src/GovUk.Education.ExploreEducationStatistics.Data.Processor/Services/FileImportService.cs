using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
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
using static GovUk.Education.ExploreEducationStatistics.Common.Services.IStatus;

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

        public async Task ImportObservations(ImportObservationsMessage message, StatisticsDbContext context)
        {
            var releaseId = message.ReleaseId;

            var status = await _importStatusService.GetImportStatus(releaseId, message.DataFileName);

            if (status.IsFinished())
            {
                _logger.LogInformation($"Import for {message.DataFileName} already finished with state " +
                                       $"{status.Status} - ignoring Observations in file {message.ObservationsFilePath}");
                return;
            }
            
            if (status.Status == CANCELLING)
            {
                _logger.LogInformation($"Import for {message.DataFileName} is CANCELLING " +
                                       $"{status.Status} - ignoring Observations in file {message.ObservationsFilePath} " +
                                       $"and marking import as CANCELLED");

                await _importStatusService.UpdateStatus(releaseId, message.DataFileName, CANCELLED, 100);
                return;
            }

            var subjectData = await _fileStorageService.GetSubjectData(message.ReleaseId, message.ObservationsFilePath);
            var releaseSubject = GetReleaseSubjectLink(message.ReleaseId, message.SubjectId, context);

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
            
            if (message.NumBatches > 1)
            {
                await _fileStorageService.DeleteBlobByPath(message.ObservationsFilePath);
            }

            await CheckComplete(releaseId, message, context);
        }

        public async Task ImportFiltersAndLocations(ImportMessage message, StatisticsDbContext context)
        {
            var dataFileBlobPath = FileStoragePathUtils.AdminReleasePath(message.Release.Id, FileType.Data, message.DataFileName);

            var subjectData = await _fileStorageService.GetSubjectData(message.Release.Id, dataFileBlobPath);
            var releaseSubject = GetReleaseSubjectLink(message.Release.Id, message.SubjectId, context);

            await using var dataFileStream = await _fileStorageService.StreamBlob(subjectData.DataBlob);
            var dataFileTable = DataTableUtils.CreateFromStream(dataFileStream);

            await using var metaFileStream = await _fileStorageService.StreamBlob(subjectData.MetaBlob);
            var metaFileTable = DataTableUtils.CreateFromStream(metaFileStream);

            await _importerService.ImportFiltersAndLocations(
                dataFileTable.Columns,
                dataFileTable.Rows,
                _importerService.GetMeta(metaFileTable, releaseSubject.Subject, context),
                context,
                message.Release.Id,
                message.DataFileName);
        }

        private static ReleaseSubject GetReleaseSubjectLink(Guid releaseId, Guid subjectId, StatisticsDbContext context)
        {
            return context
                .ReleaseSubject
                .Include(r => r.Subject)
                .Include(r => r.Release)
                .ThenInclude(r => r.Publication)
                .ThenInclude(p => p.Topic)
                .ThenInclude(t => t.Theme)
                .FirstOrDefault(r => r.Subject.Id == subjectId && r.ReleaseId == releaseId);
        }
        
        public async Task CheckComplete(Guid releaseId, ImportObservationsMessage message, StatisticsDbContext context)
        {
            var import = await _importStatusService.GetImportStatus(releaseId, message.DataFileName);

            if (import.IsFinished())
            {
                _logger.LogInformation($"Import for {message.DataFileName} is already finished in " +
                                       $"state {import.Status} - not attempting to mark as completed or failed");
                return;
            }
            
            if (import.IsAborting())
            {
                _logger.LogInformation($"Import for {message.DataFileName} is trying to abort in " +
                                       $"state {import.Status} - not attempting to mark as completed or failed, but " +
                                       $"instead marking as {import.GetFinishingStateOfAbortProcess()}, the final " +
                                       $"state of the aborting process");
                
                await _importStatusService.UpdateStatus(releaseId,
                    message.DataFileName,
                    import.GetFinishingStateOfAbortProcess(),
                    100);
                return;
            }

            if (message.NumBatches == 1 || 
                await _fileStorageService.GetNumBatchesRemaining(releaseId, message.DataFileName) == 0)
            {
                var observationCount = context.Observation.Count(o => o.SubjectId.Equals(message.SubjectId));

                if (!observationCount.Equals(message.TotalRows))
                {
                    await _batchService.FailImport(releaseId, message.DataFileName,
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
                    if (import.Errors.IsNullOrEmpty())
                    {
                        await _importStatusService.UpdateStatus(releaseId, message.DataFileName, COMPLETE, 100);
                    }
                    else
                    {
                        await _importStatusService.UpdateStatus(releaseId, message.DataFileName, FAILED, 100);
                    }
                }
            }
            else
            {
                var numBatchesRemaining =
                    await _fileStorageService.GetNumBatchesRemaining(releaseId, message.DataFileName);
                
                var percentageComplete = (double) (message.NumBatches - numBatchesRemaining) / message.NumBatches * 100;

                await _importStatusService.UpdateStatus(releaseId,
                    message.DataFileName,
                    STAGE_4,
                    percentageComplete);
            }
        }
    }
}