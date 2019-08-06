using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Importer.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using Microsoft.Azure.WebJobs;
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
        private readonly ApplicationDbContext _context;
        
        public Processor(
            IFileImportService fileImportService,
            IReleaseProcessorService releaseProcessorService,
            IFileStorageService fileStorageService,
            ISplitFileService splitFileService,
            IImporterService importerService,
            ApplicationDbContext context
        )
        {
            _fileImportService = fileImportService;
            _releaseProcessorService = releaseProcessorService;
            _fileStorageService = fileStorageService;
            _splitFileService = splitFileService;
            _importerService = importerService;
            _context = context;
        }

        [FunctionName("ProcessUploads")]
        public void ProcessUploads(
            [QueueTrigger("imports-pending", Connection = "")]
            ImportMessage message,
            ILogger logger,
            [Queue("imports-available")] ICollector<ImportMessage> collector
            )
        {
            logger.LogInformation($"{GetType().Name} function STARTED for : Datafile: {message.DataFileName}");

            try
            {
                var subjectData = ProcessSubject(message);
                
                SplitFile(message, collector, subjectData);
            }
            catch (Exception e)
            {
                logger.LogError($"{GetType().Name} function FAILED for : Datafile: {message.DataFileName} : {e.InnerException.Message} : retrying...");
                throw;
            }

            logger.LogInformation($"{GetType().Name} function COMPLETE for : Datafile: {message.DataFileName}");
        }
        
        [FunctionName("ProcessUploadsSequentially")]
        public void ProcessUploadsSequentially(
            [QueueTrigger("imports-pending-sequential", Connection = "")]
            ImportMessage[] messages,
            ILogger logger,
            [Queue("imports-available")] ICollector<ImportMessage> collector
        )
        {
            logger.LogInformation(
                $"{GetType().Name} function STARTED");

            // If processing from seeder then remove all subjects
            _context.Subject.RemoveRange(_context.Subject);
            _context.SaveChanges();

            var successes = new List<ImportMessage>();
            
            foreach (var message in messages)
            {
                try
                {
                    logger.LogInformation(
                        $"Re-seeding for : Datafile: {message.DataFileName}");
                    
                    ProcessSubject(message);
                    successes.Add(message);
                }
                catch (Exception e)
                {
                    logger.LogError(
                        $"Seeding FAILED for : Datafile: {message.DataFileName} : {e.InnerException.Message} : will not retry...");
                }

                logger.LogInformation($"First pass COMPLETE for : Datafile: {message.DataFileName}");
            }

            // Having imported all the filters etc check if file needs splitting
            foreach (var message in successes)
            {
                var subjectData = _fileStorageService.GetSubjectData(message).Result;
                SplitFile(message, collector, subjectData); 
            }

            logger.LogInformation($"{GetType().Name} function COMPLETE");
        }
        
        [FunctionName("ImportFiles")]
        public void ImportFiles(
            [QueueTrigger("imports-available", Connection = "")]
            ImportMessage message,
            ILogger logger)
        {
            try
            {
                logger.LogInformation($"{GetType().Name} function STARTED for : Batch: {message.BatchNo} of {message.BatchSize} with Datafile: {message.DataFileName}");
                
                _fileImportService.ImportObservations(message);
                
                _context.SaveChanges();

                // If the batch size is > 1 i.e. The file was split into batches
                // then delete each split batch processed
                
                if (message.BatchSize > 1)
                {
                    _fileStorageService.Delete(message);
                }
            }
            catch (Exception e)
            {
                logger.LogError(
                    $"{GetType().Name} function FAILED: : Batch: {message.BatchNo} of {message.BatchSize} with Datafile: {message.DataFileName} : {e.InnerException.Message} : retrying...");
                throw;
            }

            logger.LogInformation($"{GetType().Name} function COMPLETE for : Batch: {message.BatchNo}  of {message.BatchSize} with Datafile: {message.DataFileName}");
        }

        private SubjectData ProcessSubject(ImportMessage message)
        {
            var subjectData = _fileStorageService.GetSubjectData(message).Result;
                
            var subject =_releaseProcessorService.CreateOrUpdateRelease(subjectData, message);
                
            _context.SaveChanges();

            _importerService.ImportMeta(subjectData.GetMetaLines().ToList(), subject);
                
            _context.SaveChanges();
                
            _fileImportService.ImportFiltersLocationsAndSchools(message);

            _context.SaveChanges();

            return subjectData;
        }

        private void SplitFile(ImportMessage message, ICollector<ImportMessage> collector, SubjectData subjectData)
        {
            _splitFileService.SplitDataFile(collector, message, subjectData);
        }
    }
}