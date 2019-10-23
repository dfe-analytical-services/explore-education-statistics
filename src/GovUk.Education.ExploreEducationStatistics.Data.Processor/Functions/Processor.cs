using System;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Importer.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Functions
{
    // ReSharper disable once UnusedMember.Global
    public class Processor
    {
        private readonly IFileImportService _fileImportService;
        private readonly IReleaseProcessorService _releaseProcessorService;
        private readonly IFileStorageService _fileStorageService;
        private readonly ISplitFileService _splitFileService;
        private readonly IImporterService _importerService;
        private readonly IBatchService _batchService;
        private readonly StatisticsDbContext _context;
        private readonly IValidatorService _validatorService;
        
        public Processor(
            IFileImportService fileImportService,
            IReleaseProcessorService releaseProcessorService,
            IFileStorageService fileStorageService,
            ISplitFileService splitFileService,
            IImporterService importerService,
            IBatchService batchService,
            StatisticsDbContext context,
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
            _context = context;
        }

        [FunctionName("ProcessUploads")]
        public void ProcessUploads(
            [QueueTrigger("imports-pending")]
            ImportMessage message,
            ILogger logger,
            ExecutionContext context,
            [Queue("imports-available")] ICollector<ImportMessage> collector
            )
        {
            var batchSettings = GetBatchSettings(LoadAppSettings(context));
            
            if (IsDataFileValid(message, batchSettings.RowsPerBatch, logger)) {
                
                try
                {
                    var subjectData = ProcessSubject(message);
                    _splitFileService.SplitDataFile(collector, message, subjectData, batchSettings);
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
        public void ProcessUploadsSequentially(
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
                if (!IsDataFileValid(message, batchSettings.RowsPerBatch, logger))
                {
                    allValid = false;
                }
            }

            if (allValid)
            {
                foreach (var message in messages)
                {
                    logger.LogInformation($"Re-seeding for : Datafile: {message.DataFileName}");
                    ProcessSubject(message);
                    var subjectData = _fileStorageService.GetSubjectData(message).Result;
                    _splitFileService.SplitDataFile(collector, message, subjectData, batchSettings);
                    logger.LogInformation($"First pass COMPLETE for : Datafile: {message.DataFileName}");
                }
            }
            else
            {
                logger.LogError(
                    $"Seeding FAILED...check log"); 
            }
        }
        
        [FunctionName("ImportObservations")]
        public void ImportObservations(
            [QueueTrigger("imports-available")]
            ImportMessage message,
            ILogger logger)
        {
            _fileImportService.ImportObservations(message).Wait();
        }

        private SubjectData ProcessSubject(ImportMessage message)
        {
            var subjectData = _fileStorageService.GetSubjectData(message).Result;
            var subject =_releaseProcessorService.CreateOrUpdateRelease(subjectData, message);
            
            SaveChanges();

            _importerService.ImportMeta(subjectData.GetMetaLines().ToList(), subject);
                
            SaveChanges();
                
            _fileImportService.ImportFiltersLocationsAndSchools(message);

            SaveChanges();

            return subjectData;
        }

        private bool IsDataFileValid(ImportMessage message, int rowsPerBatch, ILogger logger)
        {
            logger.LogInformation($"Validating Datafile: {message.DataFileName}");

            var subjectData = _fileStorageService.GetSubjectData(message).Result;

            var numberOfRows = subjectData.GetCsvLines().Count();
            
            _batchService.CreateImport(
                message.Release.Id.ToString(), 
                message.DataFileName,
                numberOfRows,
                SplitFileService.GetNumBatches(numberOfRows, rowsPerBatch)
                
                ).Wait();
            
            var errors = _validatorService.Validate(message, subjectData);
            
            if (errors.Count > 0)
            {
                logger.LogInformation($"Datafile: {message.DataFileName} has errors");

                _batchService.FailImport(
                    message.Release.Id.ToString(),
                    message.DataFileName,
                    errors
                    ).Wait();
                
                return false;
            }

            return true;
        }

        private void SaveChanges()
        {
            _context.SaveChanges();
        }
        
        private static IConfigurationRoot LoadAppSettings(ExecutionContext context)
        {
            return new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();
        }

        private static BatchSettings GetBatchSettings(IConfigurationRoot config)
        {
            return new BatchSettings {RowsPerBatch = Convert.ToInt32(config.GetValue<string>("RowsPerBatch"))};
        }
    }
}