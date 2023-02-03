using System;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.DataImportStatus;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services
{
    public class FileImportService : IFileImportService
    {
        private readonly ILogger<FileImportService> _logger;
        private readonly IBatchService _batchService;
        private readonly IBlobStorageService _blobStorageService;
        private readonly IDataImportService _dataImportService;
        private readonly IImporterService _importerService;
        private readonly IDatabaseHelper _databaseHelper;

        public FileImportService(
            ILogger<FileImportService> logger,
            IBatchService batchService,
            IBlobStorageService blobStorageService,
            IDataImportService dataImportService,
            IImporterService importerService, 
            IDatabaseHelper databaseHelper)
        {
            _logger = logger;
            _batchService = batchService;
            _blobStorageService = blobStorageService;
            _dataImportService = dataImportService;
            _importerService = importerService;
            _databaseHelper = databaseHelper;
        }

        public async Task ImportObservations(ImportObservationsMessage message, StatisticsDbContext context)
        {
            var import = await _dataImportService.GetImport(message.Id);

            _logger.LogInformation($"Importing Observations for {import.File.Filename} batchNo {message.BatchNo}");

            if (import.Status.IsFinished())
            {
                _logger.LogInformation($"Import for {import.File.Filename} already finished with state " +
                                       $"{import.Status} - ignoring Observations in file {message.ObservationsFilePath}");
                return;
            }

            if (import.Status == CANCELLING)
            {
                _logger.LogInformation($"Import for {import.File.Filename} is " +
                                       $"{import.Status} - ignoring Observations in file {message.ObservationsFilePath} " +
                                       "and marking import as CANCELLED");

                await _dataImportService.UpdateStatus(message.Id, CANCELLED, 100);
                return;
            }

            var subject = await context.Subject.FindAsync(import.SubjectId);

            var datafileStreamProvider = () => _blobStorageService.StreamBlob(PrivateReleaseFiles, message.ObservationsFilePath);
            var metaFileStreamProvider = () => _blobStorageService.StreamBlob(PrivateReleaseFiles, import.MetaFile.Path());

            var metaFileCsvHeaders = await CsvUtils.GetCsvHeaders(metaFileStreamProvider);
            var metaFileCsvRows = await CsvUtils.GetCsvRows(metaFileStreamProvider);

            await _databaseHelper.DoInTransaction(
                context,
                async ctxDelegate =>
                {
                    await _importerService.ImportObservations(
                        import,
                        datafileStreamProvider,
                        subject,
                        _importerService.GetMeta(metaFileCsvHeaders, metaFileCsvRows, subject, ctxDelegate),
                        message.BatchNo,
                        ctxDelegate
                    );
                });

            if (import.NumBatches > 1)
            {
                await _blobStorageService.DeleteBlob(PrivateReleaseFiles, message.ObservationsFilePath);
            }

            await CheckComplete(message, context);
        }

        public async Task ImportFiltersAndLocations(Guid importId, StatisticsDbContext context)
        {
            var import = await _dataImportService.GetImport(importId);

            var subject = await context.Subject.FindAsync(import.SubjectId);

            var datafileStreamProvider = () => _blobStorageService.StreamBlob(PrivateReleaseFiles, import.File.Path());
            var metaFileStreamProvider = () => _blobStorageService.StreamBlob(PrivateReleaseFiles, import.MetaFile.Path());

            var metaFileCsvHeaders = await CsvUtils.GetCsvHeaders(metaFileStreamProvider);
            var metaFileCsvRows = await CsvUtils.GetCsvRows(metaFileStreamProvider);

            await _importerService.ImportFiltersAndLocations(
                import,
                datafileStreamProvider,
                _importerService.GetMeta(metaFileCsvHeaders, metaFileCsvRows, subject, context),
                context);
        }

        public async Task CheckComplete(ImportObservationsMessage message, StatisticsDbContext context)
        {
            var import = await _dataImportService.GetImport(message.Id);

            if (import.Status.IsFinished())
            {
                _logger.LogInformation($"Import for {import.File.Filename} is already finished in " +
                                       $"state {import.Status} - not attempting to mark as completed or failed");
                return;
            }

            if (import.Status.IsAborting())
            {
                _logger.LogInformation($"Import for {import.File.Filename} is trying to abort in " +
                                       $"state {import.Status} - not attempting to mark as completed or failed, but " +
                                       $"instead marking as {import.Status.GetFinishingStateOfAbortProcess()}, the final " +
                                       $"state of the aborting process");

                await _dataImportService.UpdateStatus(message.Id, import.Status.GetFinishingStateOfAbortProcess(), 100);
                return;
            }

            if (!import.BatchingRequired() || await _batchService.GetNumBatchesRemaining(import.File) == 0)
            {
                var observationCount = context.Observation.Count(o => o.SubjectId.Equals(import.SubjectId));

                if (!observationCount.Equals(import.ImportedRows))
                {
                    await _dataImportService.FailImport(message.Id,
                        $"Number of observations inserted ({observationCount}) " +
                                $"does not equal that expected ({import.ImportedRows}) : Please delete & retry");
                }
                else
                {
                    if (import.Errors.Count == 0)
                    {
                        await _dataImportService.UpdateStatus(message.Id, COMPLETE, 100);
                    }
                    else
                    {
                        await _dataImportService.UpdateStatus(message.Id, FAILED, 100);
                    }
                }
            }
            else
            {
                var numBatchesRemaining = await _batchService.GetNumBatchesRemaining(import.File);

                var percentageComplete = (double) (import.NumBatches - numBatchesRemaining) / import.NumBatches * 100;

                await _dataImportService.UpdateStatus(message.Id, STAGE_4, percentageComplete);
            }
        }
    }
}
