using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Importer.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Importer.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Utils;
using Microsoft.Azure.WebJobs;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Functions
{
    // ReSharper disable once UnusedMember.Global
    public class Processor
    {
        private readonly IBatchService _batchService;
        private readonly IFileImportService _fileImportService;
        private readonly IFileStorageService _fileStorageService;
        private readonly IImporterService _importerService;
        private readonly IReleaseProcessorService _releaseProcessorService;
        private readonly ISplitFileService _splitFileService;
        private readonly IValidatorService _validatorService;
        private readonly IDataArchiveService _dataArchiveService;
        private readonly IImportStatusService _importStatusService;

        public Processor(
            IFileImportService fileImportService,
            IReleaseProcessorService releaseProcessorService,
            IFileStorageService fileStorageService,
            ISplitFileService splitFileService,
            IImporterService importerService,
            IBatchService batchService,
            IValidatorService validatorService,
            IDataArchiveService dataArchiveService,
            IImportStatusService importStatusService
        )
        {
            _fileImportService = fileImportService;
            _releaseProcessorService = releaseProcessorService;
            _fileStorageService = fileStorageService;
            _splitFileService = splitFileService;
            _importerService = importerService;
            _batchService = batchService;
            _validatorService = validatorService;
            _dataArchiveService = dataArchiveService;
            _importStatusService = importStatusService;
        }

        [FunctionName("ProcessUploads")]
        public async void ProcessUploads(
            [QueueTrigger("imports-pending")] ImportMessage message,
            ILogger logger,
            ExecutionContext executionContext,
            [Queue("imports-available")] ICollector<ImportMessage> collector
        )
        {
            try
            {
                await ProcessUnpackingArchive(message, logger);

                var subjectData = await _fileStorageService.GetSubjectData(message);
                
                await ProcessStage1(message, logger, executionContext, subjectData);
                await ProcessStage2(message, logger, subjectData);
                await ProcessStage3(message, logger, subjectData);
                await ProcessStage4Messages(message, logger, collector);
            }
            catch (Exception e)
            {
                var ex = GetInnerException(e);

                await _batchService.FailImport(message.Release.Id,
                    message.OrigDataFileName,
                    new List<ValidationError>
                    {
                        new ValidationError(ex.Message)
                    });

                logger.LogError(ex, $"{GetType().Name} function FAILED for : Datafile: " +
                                    $"{message.DataFileName} : {ex.Message}");
                logger.LogError(ex.StackTrace);
            }
        }

        [FunctionName("ImportObservations")]
        public async Task ImportObservations(
            [QueueTrigger("imports-available")] ImportMessage message,
            ILogger logger)
        {
            try
            {
                await _fileImportService.ImportObservations(message, DbUtils.CreateStatisticsDbContext());
            }
            catch (Exception e)
            {
                // If deadlock exception then throw & try up to 3 times
                if (e is SqlException exception && exception.Number == 1205)
                {
                    logger.LogInformation($"{GetType().Name} : Handling known exception when processing Datafile: " +
                                          $"{message.DataFileName} : {exception.Message} : transaction will be retried");
                    throw;
                }

                var ex = GetInnerException(e);

                await _batchService.FailImport(message.Release.Id,
                    message.OrigDataFileName,
                    new List<ValidationError>
                    {
                        new ValidationError(ex.Message)
                    });

                logger.LogError(ex, $"{GetType().Name} function FAILED for : Datafile: " +
                                    $"{message.DataFileName} : {ex.Message}");
            }
        }

        private async Task ProcessUnpackingArchive(
            ImportMessage message,
            ILogger logger)
        {
            if (message.ArchiveFileName != "")
            {
                logger.LogInformation($"Unpacking archive for {message.DataFileName}");

                var status =
                    await _importStatusService.GetImportStatus(message.Release.Id, message.OrigDataFileName);

                if (status.IsAfterArchiveProcessing())
                {
                    logger.LogInformation($"Unpacking archive already completed for {message.DataFileName} - skipping");
                    return;
                }
                
                await _importStatusService.UpdateStatus(message.Release.Id,
                    message.DataFileName,
                    IStatus.PROCESSING_ARCHIVE_FILE);

                await _dataArchiveService.ExtractDataFiles(message.Release.Id, message.ArchiveFileName);
            }
        }

        private async Task ProcessStage1(
            ImportMessage message,
            ILogger logger,
            ExecutionContext executionContext,
            SubjectData subjectData)
        {
            logger.LogInformation($"Processing Stage 1 for {message.DataFileName}");

            var status =
                await _importStatusService.GetImportStatus(message.Release.Id, message.OrigDataFileName);

            if (status.IsAfterStage1())
            {
                logger.LogInformation($"Stage 1 already completed for {message.DataFileName} - skipping");
                return;
            }
            
            await _importStatusService.UpdateStatus(message.Release.Id, message.DataFileName, IStatus.STAGE_1);

            await _validatorService.Validate(message.Release.Id, subjectData, executionContext, message)
                .OnSuccess(async result =>
                {
                    message.RowsPerBatch = result.RowsPerBatch;
                    message.TotalRows = result.FilteredObservationCount;
                    message.NumBatches = result.NumBatches;
                    await _batchService.UpdateStoredMessage(message);
                })
                .OnFailureDo(async errors =>
                {
                    await _batchService.FailImport(message.Release.Id,
                        message.OrigDataFileName,
                        errors);

                    logger.LogError($"Import FAILED for {message.DataFileName}...check log");
                });
        }

        private async Task ProcessStage2(
            ImportMessage message,
            ILogger logger,
            SubjectData subjectData)
        {
            logger.LogInformation($"Processing Stage 2 for {message.DataFileName}");

            var status =
                await _importStatusService.GetImportStatus(message.Release.Id, message.OrigDataFileName);

            if (status.IsAfterStage2())
            {
                logger.LogInformation($"Stage 2 already completed for {message.DataFileName} - skipping");
                return;
            }

            await _importStatusService.UpdateStatus(message.Release.Id,
                message.OrigDataFileName,
                IStatus.STAGE_2);

            await ProcessSubject(message,
                DbUtils.CreateStatisticsDbContext(),
                DbUtils.CreateContentDbContext(),
                subjectData);
        }

        private async Task ProcessStage3(
            ImportMessage message, 
            ILogger logger,
            SubjectData subjectData)
        {
            logger.LogInformation($"Processing Stage 3 for {message.DataFileName}");

            var status =
                await _importStatusService.GetImportStatus(message.Release.Id, message.OrigDataFileName);

            if (status.IsAfterStage3())
            {
                logger.LogInformation($"Stage 3 already completed for {message.DataFileName} - skipping");
                return;
            }

            await _splitFileService.SplitDataFile(message, subjectData);
        }
        
        

        private async Task ProcessStage4Messages(
            ImportMessage message, 
            ILogger logger,
            ICollector<ImportMessage> collector)
        {
            logger.LogInformation($"Processing Stage 4 message creation for {message.DataFileName}");

            var status =
                await _importStatusService.GetImportStatus(message.Release.Id, message.OrigDataFileName);

            if (status.IsAfterStage4())
            {
                logger.LogInformation($"Stage 4 already completed for {message.DataFileName} - skipping");
                return;
            }

            await _splitFileService.CreateDataFileProcessingMessages(collector, message);
        }
        
        private async Task ProcessSubject(
            ImportMessage message,
            StatisticsDbContext statisticsDbContext,
            ContentDbContext contentDbContext,
            SubjectData subjectData)
        {
            var subject = _releaseProcessorService.CreateOrUpdateRelease(subjectData,
                    message,
                    statisticsDbContext,
                    contentDbContext);

            await using var metaFileStream = await _fileStorageService.StreamBlob(subjectData.MetaBlob);
            var metaFileTable = DataTableUtils.CreateFromStream(metaFileStream);

            _importerService.ImportMeta(metaFileTable, subject, statisticsDbContext);

            await statisticsDbContext.SaveChangesAsync();

            await _fileImportService.ImportFiltersLocationsAndSchools(message, statisticsDbContext);

            await statisticsDbContext.SaveChangesAsync();
        }

        private static Exception GetInnerException(Exception ex)
        {
            return ex.InnerException ?? ex;
        }
    }
}