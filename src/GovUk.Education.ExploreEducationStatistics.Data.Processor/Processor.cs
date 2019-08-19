using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Importer.Exceptions;
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

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor
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
        private readonly ApplicationDbContext _context;
        
        public Processor(
            IFileImportService fileImportService,
            IReleaseProcessorService releaseProcessorService,
            IFileStorageService fileStorageService,
            ISplitFileService splitFileService,
            IImporterService importerService,
            IBatchService batchService,
            ApplicationDbContext context
        )
        {
            _fileImportService = fileImportService;
            _releaseProcessorService = releaseProcessorService;
            _fileStorageService = fileStorageService;
            _splitFileService = splitFileService;
            _importerService = importerService;
            _batchService = batchService;
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
            logger.LogInformation($"{GetType().Name} function STARTED for : Datafile: {message.DataFileName}");
            
            var batchSettings = GetBatchSettings(LoadAppSettings(context));
            
            try
            {
                var subjectData = ProcessSubject(message, batchSettings.BatchSize);
                SplitFile(message, collector, subjectData, batchSettings);
            }
            catch (Exception e)
            {
                // TODO - We need the subject ID to properly log an unknown error
                logger.LogError($"{GetType().Name} function FAILED for : Datafile: {message.DataFileName} : {e.Message} : will retry unknown exceptions 3 times...");

                // If it's known exception then log it but don't throw
                if (e is ImporterException ex)
                {
                    _batchService.FailBatch(message.Release.Id.ToString(), ex.SubjectId.ToString(),
                        new List<String> {ex.Message});
                }
                else
                {
                    throw;
                }
            }

            logger.LogInformation($"{GetType().Name} function COMPLETE for : Datafile: {message.DataFileName}");
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
            logger.LogInformation($"{GetType().Name} function STARTED");
            
            var batchSettings = GetBatchSettings(LoadAppSettings(context));
            var successes = new List<ImportMessage>();
            
            foreach (var message in messages)
            {
                try
                {
                    logger.LogInformation($"Re-seeding for : Datafile: {message.DataFileName}");
                    ProcessSubject(message, batchSettings.BatchSize);
                    successes.Add(message);
                }
                catch (Exception e)
                {
                    // If it's known exception then log it
                    if (e is ImporterException ex)
                    {
                        _batchService.FailBatch(message.Release.Id.ToString(), ex.SubjectId.ToString(), new List<String>{ex.Message}).Wait();
                    }
                    logger.LogError(
                        $"Seeding FAILED for : Datafile: {message.DataFileName} : {e.Message} : will not retry...");
                    // As this is the seeder continue with the next one - don't throw!
                    continue;
                }

                logger.LogInformation($"First pass COMPLETE for : Datafile: {message.DataFileName}");
            }

            // Having imported all the filters etc check if file needs splitting
            foreach (var message in successes)
            {
                var subjectData = _fileStorageService.GetSubjectData(message).Result;
                SplitFile(message, collector, subjectData, batchSettings); 
            }

            logger.LogInformation($"{GetType().Name} function COMPLETE");
        }
        
        [FunctionName("ImportObservations")]
        public void ImportObservations(
            [QueueTrigger("imports-available")]
            ImportMessage message,
            ILogger logger)
        {
            logger.LogInformation(
                    $"{GetType().Name} function STARTED for : Batch: {message.BatchNo} of {message.BatchSize} with Datafile: {message.DataFileName}");
                
            _fileImportService.ImportObservations(message).Wait();

            logger.LogInformation(
                $"{GetType().Name} function COMPLETE for : Batch: {message.BatchNo}  of {message.BatchSize} with Datafile: {message.DataFileName}");
        }

        private SubjectData ProcessSubject(ImportMessage message, int batchSize)
        {
            var subjectData = _fileStorageService.GetSubjectData(message).Result;
            var subject =_releaseProcessorService.CreateOrUpdateRelease(subjectData, message);
            
            SaveChanges();
            
            var numBatches = SplitFileService.GetNumBatches(subjectData.GetCsvLines().Count(), batchSize);
            
            _batchService.UpdateStatus(message.Release.Id.ToString(), subject.Id.ToString(), numBatches, ImportStatus.RUNNING_PHASE_1).Wait();

            _importerService.ImportMeta(subjectData.GetMetaLines().ToList(), subject);
                
            SaveChanges();
                
            _fileImportService.ImportFiltersLocationsAndSchools(message);

            SaveChanges();

            return subjectData;
        }

        private void SplitFile(
            ImportMessage message,
            ICollector<ImportMessage> collector,
            SubjectData subjectData,
            BatchSettings batchSettings)
        {
            _splitFileService.SplitDataFile(collector, message, subjectData, batchSettings);
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
            var batchSettings = new BatchSettings {BatchSize = config.GetValue<int>("BatchSize")};
            return batchSettings;
        }
    }
}