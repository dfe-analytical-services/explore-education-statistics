using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
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

        public Processor(
            IFileImportService fileImportService,
            IReleaseProcessorService releaseProcessorService,
            IFileStorageService fileStorageService,
            ISplitFileService splitFileService,
            IImporterService importerService,
            IBatchService batchService,
            IValidatorService validatorService,
            IDataArchiveService dataArchiveService
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
        }

        [FunctionName("ProcessUploads")]
        public async void ProcessUploads(
            [QueueTrigger("imports-pending")] ImportMessage message,
            ILogger logger,
            ExecutionContext executionContext,
            [Queue("imports-available")] ICollector<ImportMessage> collector
        )
        {
            if (message.Seeding)
            {
                await _batchService.CreateImport(message.Release.Id.ToString(), message.DataFileName, 0, message);
            }

            if (message.ArchiveFileName != "")
            {
                await _batchService.UpdateStatus(message.Release.Id.ToString(), message.DataFileName, IStatus.PROCESSING_ARCHIVE_FILE);
                await _dataArchiveService.ExtractDataFiles(message.Release.Id, message.ArchiveFileName);
            }

            await _batchService.UpdateStatus(message.Release.Id.ToString(), message.DataFileName, IStatus.RUNNING_PHASE_1);
            var subjectData = await _fileStorageService.GetSubjectData(message);

            await _validatorService.Validate(message.Release.Id, subjectData, executionContext, message)
                .OnSuccess(async result =>
                {
                    try
                    {
                        message.RowsPerBatch = result.RowsPerBatch;
                        message.TotalRows = result.FilteredObservationCount;
                        message.NumBatches = result.NumBatches;

                        await _batchService.UpdateStoredMessage(message);
                        await ProcessSubject(message, DbUtils.CreateStatisticsDbContext(), DbUtils.CreateContentDbContext(), subjectData);
                        await _splitFileService.SplitDataFile(collector, message, subjectData);
                    }
                    catch (Exception e)
                    {
                        var ex = GetInnerException(e);

                        await _batchService.FailImport(
                            message.Release.Id.ToString(),
                            message.OrigDataFileName,
                            new List<ValidationError> {new ValidationError(ex.Message)}
                        );

                        logger.LogError(ex,$"{GetType().Name} function FAILED for : Datafile: " +
                                           $"{message.DataFileName} : {ex.Message}");
                        logger.LogError(ex.StackTrace);
                    }

                    return true;
                })
                .OnFailureDo(async errors =>
                {
                    await _batchService.FailImport(
                        message.Release.Id.ToString(),
                        message.OrigDataFileName,
                        errors
                    );

                    logger.LogError($"Import FAILED for {message.DataFileName}...check log");
                });
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

                await _batchService.FailImport(
                    message.Release.Id.ToString(),
                    message.OrigDataFileName,
                    new List<ValidationError> {new ValidationError(ex.Message)}
                );

                logger.LogError(ex,$"{GetType().Name} function FAILED for : Datafile: " +
                                $"{message.DataFileName} : {ex.Message}");
            }
        }

        private async Task ProcessSubject(
            ImportMessage message,
            StatisticsDbContext statisticsDbContext,
            ContentDbContext contentDbContext,
            SubjectData subjectData)
        {
            var status = await _batchService.GetStatus(message.Release.Id.ToString(), message.OrigDataFileName);

            // If already reached Phase 2 then don't re-create the subject
            if ((int)status > (int)IStatus.RUNNING_PHASE_1)
            {
                return;
            }

            var subject = _releaseProcessorService.CreateOrUpdateRelease(subjectData, message, statisticsDbContext, contentDbContext);

            await using var metaFileStream = await _fileStorageService.StreamBlob(subjectData.MetaBlob);
            var metaFileTable = DataTableUtils.CreateFromStream(metaFileStream);

            _importerService.ImportMeta(metaFileTable, subject, statisticsDbContext);

            if (message.Seeding)
            {
                SampleGuids.GenerateIndicatorGuids(statisticsDbContext);
                SampleGuids.GenerateFilterGuids(statisticsDbContext);
                SampleGuids.GenerateFilterGroupGuids(statisticsDbContext);
            }

            await statisticsDbContext.SaveChangesAsync();

            await _fileImportService.ImportFiltersLocationsAndSchools(message, statisticsDbContext);

            if (message.Seeding)
            {
                SampleGuids.GenerateFilterGuids(statisticsDbContext);
                SampleGuids.GenerateFilterGroupGuids(statisticsDbContext);
                SampleGuids.GenerateFilterItemGuids(statisticsDbContext);
            }

            await statisticsDbContext.SaveChangesAsync();
            await _batchService.UpdateStatus(message.Release.Id.ToString(), message.OrigDataFileName,
                IStatus.RUNNING_PHASE_2);
        }

        private static Exception GetInnerException(Exception ex)
        {
            return ex.InnerException ?? ex;
        }
    }
}