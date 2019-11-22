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
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

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
            var batchSettings = GetBatchSettings(LoadAppSettings(context));

            if (await IsDataFileValid(message, batchSettings.RowsPerBatch, logger))
            {
                try
                {
                    var subjectData = await ProcessSubject(message, DbUtils.CreateDbContext());
                    await _splitFileService.SplitDataFile(collector, message, subjectData, batchSettings);
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
                if (!await IsDataFileValid(message, batchSettings.RowsPerBatch, logger))
                {
                    allValid = false;
                }

            if (allValid)
            {
                foreach (var message in messages)
                {
                    logger.LogInformation($"Re-seeding for : Datafile: {message.DataFileName}");
                    await ProcessSubject(message, DbUtils.CreateDbContext());
                    var subjectData = await _fileStorageService.GetSubjectData(message);
                    await _splitFileService.SplitDataFile(collector, message, subjectData, batchSettings);
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

        private async Task<SubjectData> ProcessSubject(ImportMessage message, StatisticsDbContext dbContext)
        {
            await _batchService.UpdateStatus(message.Release.Id.ToString(), message.OrigDataFileName,
                IStatus.RUNNING_PHASE_2);

            var subjectData = await _fileStorageService.GetSubjectData(message);
            var subject = _releaseProcessorService.CreateOrUpdateRelease(subjectData, message, dbContext);

            await dbContext.SaveChangesAsync();

            _importerService.ImportMeta(subjectData.GetMetaLines().ToList(), subject, dbContext);

            await dbContext.SaveChangesAsync();

            _fileImportService.ImportFiltersLocationsAndSchools(message, dbContext);

            await dbContext.SaveChangesAsync();

            return subjectData;
        }

        private async Task<bool> IsDataFileValid(ImportMessage message, int rowsPerBatch, ILogger logger)
        {
            logger.LogInformation($"Validating Datafile: {message.DataFileName}");

            var subjectData = await _fileStorageService.GetSubjectData(message);

            var numberOfRows = subjectData.GetCsvLines().Count();

            await _batchService.CreateImport(
                message.Release.Id.ToString(),
                message.DataFileName,
                numberOfRows,
                SplitFileService.GetNumBatches(numberOfRows, rowsPerBatch),
                message
            );

            var errors = _validatorService.Validate(message, subjectData);

            if (errors.Count <= 0)
            {
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