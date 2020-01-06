using System;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Importer.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Importer.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Utils;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
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

        public Processor(
            IFileImportService fileImportService,
            IReleaseProcessorService releaseProcessorService,
            IFileStorageService fileStorageService,
            ISplitFileService splitFileService,
            IImporterService importerService,
            IBatchService batchService,
            IValidatorService validatorService
        )
        {
            _fileImportService = fileImportService;
            _releaseProcessorService = releaseProcessorService;
            _fileStorageService = fileStorageService;
            _splitFileService = splitFileService;
            _importerService = importerService;
            _batchService = batchService;
            _validatorService = validatorService;
        }

        [FunctionName("ProcessUploads")]
        public async void ProcessUploads(
            [QueueTrigger("imports-pending")] ImportMessage message,
            ILogger logger,
            ExecutionContext context,
            [Queue("imports-available")] ICollector<ImportMessage> collector
        )
        {
            message.RowsPerBatch = GetBatchSettings(LoadAppSettings(context)).RowsPerBatch;
            
            if (await IsDataFileValid(message, logger))
            {
                try
                {
                    var subjectData = await ProcessSubject(message, DbUtils.CreateDbContext(), false);
                    logger.LogInformation($"Splitting Datafile: {message.DataFileName} if > {message.RowsPerBatch} lines");
                    await _splitFileService.SplitDataFile(collector, message, subjectData);
                    logger.LogInformation($"Split of Datafile: {message.DataFileName} - complete");
                }
                catch (Exception e)
                {
                    logger.LogError($"{GetType().Name} function FAILED for : Datafile: " +
                                    $"{message.DataFileName} : {e.Message} : will retry unknown exceptions 3 times...");
                    throw;
                }
            }
            else
            {
                logger.LogError(
                    $"Import FAILED for {message.DataFileName}...check log");
            }
        }

        [FunctionName("ProcessUploadsSequentially")]
        public async void ProcessUploadsSequentially(
            [QueueTrigger("imports-pending-sequential")]
            ImportMessage[] messages,
            ILogger logger,
            ExecutionContext context,
            [Queue("imports-available")] ICollector<ImportMessage> collector
        )
        {
            var batchSettings = GetBatchSettings(LoadAppSettings(context));

            // Log all initial validation messages
            var allValid = true;
            foreach (var message in messages)
            {
                message.RowsPerBatch = batchSettings.RowsPerBatch;
                if (!await IsDataFileValid(message, logger))
                {
                    allValid = false;
                }
            }

            if (allValid)
            {
                foreach (var message in messages)
                {
                    logger.LogInformation($"Re-seeding for : Datafile: {message.DataFileName}");
                    await ProcessSubject(message, DbUtils.CreateDbContext(), true);
                    var subjectData = await _fileStorageService.GetSubjectData(message);
                    await _splitFileService.SplitDataFile(collector, message, subjectData);
                    logger.LogInformation($"First pass COMPLETE for : Datafile: {message.DataFileName}");
                }
            }
            else
            {
                logger.LogError(
                    "Seeding FAILED...check log");
            }
        }

        [FunctionName("ImportObservations")]
        public async Task ImportObservations(
            [QueueTrigger("imports-available")] ImportMessage message)
        {
            await _fileImportService.ImportObservations(message, DbUtils.CreateDbContext());
        }
        
        private async Task<SubjectData> ProcessSubject(ImportMessage message, StatisticsDbContext dbContext, bool isSeeding)
        {
            var status = await _batchService.GetStatus(message.Release.Id.ToString(), message.OrigDataFileName);
            
            // If already reached Phase 2 then don't re-create the subject
            if ((int)status > (int)IStatus.RUNNING_PHASE_1)
            {
                return await _fileStorageService.GetSubjectData(message); 
            }

            var subjectData = await _fileStorageService.GetSubjectData(message);
            var subject = _releaseProcessorService.CreateOrUpdateRelease(subjectData, message, dbContext);

            await dbContext.SaveChangesAsync();

            _importerService.ImportMeta(subjectData.GetMetaLines().ToList(), subject, dbContext);

            if (isSeeding)
            {
                SampleGuids.GenerateIndicatorGuids(dbContext);
                SampleGuids.GenerateFilterGuids(dbContext);
                SampleGuids.GenerateFilterGroupGuids(dbContext);
            }

            await dbContext.SaveChangesAsync();

            _fileImportService.ImportFiltersLocationsAndSchools(message, dbContext);
            
            if (isSeeding)
            {
                SampleGuids.GenerateFilterGuids(dbContext);
                SampleGuids.GenerateFilterGroupGuids(dbContext);
                SampleGuids.GenerateFilterItemGuids(dbContext);
            }

            await dbContext.SaveChangesAsync();
            
            await _batchService.UpdateStatus(message.Release.Id.ToString(), message.OrigDataFileName,
                IStatus.RUNNING_PHASE_2);

            return subjectData;
        }

        private async Task<bool> IsDataFileValid(ImportMessage message, ILogger logger)
        {
            logger.LogInformation($"Validating Datafile: {message.DataFileName}");

            var subjectData = await _fileStorageService.GetSubjectData(message);

            var numberOfRows = subjectData.GetCsvLines().Count();

            await _batchService.CreateImport(
                message.Release.Id.ToString(),
                message.DataFileName,
                numberOfRows,
                SplitFileService.GetNumBatches(numberOfRows, message.RowsPerBatch),
                message
            );

            var errors = _validatorService.Validate(message, subjectData);

            if (errors.Count <= 0)
            {
                logger.LogInformation($"Validating Datafile: {message.DataFileName} - complete");
                return true;
            }

            logger.LogInformation($"Datafile: {message.DataFileName} has errors");

            await _batchService.FailImport(
                message.Release.Id.ToString(),
                message.DataFileName,
                errors
            );

            return false;
        }

        private static IConfigurationRoot LoadAppSettings(ExecutionContext context)
        {
            return new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", true, true)
                .AddEnvironmentVariables()
                .Build();
        }

        private static BatchSettings GetBatchSettings(IConfigurationRoot config)
        {
            return new BatchSettings
            {
                RowsPerBatch =
                    Convert.ToInt32(config.GetValue<string>("RowsPerBatch"))
            };
        }
    }
}