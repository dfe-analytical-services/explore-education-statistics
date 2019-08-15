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
            [Queue("imports-available")] ICollector<ImportMessage> collector
            )
        {
            logger.LogInformation($"{GetType().Name} function STARTED for : Datafile: {message.DataFileName}");

            try
            {
                var subjectData = ProcessSubject(message);
                
                SplitFile(message, collector, subjectData);
            }
            catch (ImporterException ex)
            {
                _batchService.FailBatch(message.Release.Id.ToString(), ex._subjectId.ToString(), ex.Message);
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
            [QueueTrigger("imports-pending-sequential")]
            ImportMessage[] messages,
            ILogger logger,
            [Queue("imports-available")] ICollector<ImportMessage> collector
        )
        {
            logger.LogInformation($"{GetType().Name} function STARTED");

            var successes = new List<ImportMessage>();
            
            foreach (var message in messages)
            {
                try
                {
                    logger.LogInformation($"Re-seeding for : Datafile: {message.DataFileName}");
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
        
        [FunctionName("ImportObservations")]
        public void ImportObservations(
            [QueueTrigger("imports-available")]
            ImportMessage message,
            ILogger logger)
        {
            try
            {
                logger.LogInformation(
                    $"{GetType().Name} function STARTED for : Batch: {message.BatchNo} of {message.BatchSize} with Datafile: {message.DataFileName}");

                _fileImportService.ImportObservations(message).Wait();
            }
            catch (ImporterException ex)
            {
                _batchService.FailBatch(message.Release.Id.ToString(), ex._subjectId.ToString(), ex.Message);
            }
            catch (Exception e)
            {
                logger.LogError(
                    $"{GetType().Name} function FAILED: : Batch: {message.BatchNo} of {message.BatchSize} with Datafile: {message.DataFileName} : {e.InnerException.Message} : retrying...");
                throw;
            }

            logger.LogInformation(
                $"{GetType().Name} function COMPLETE for : Batch: {message.BatchNo}  of {message.BatchSize} with Datafile: {message.DataFileName}");
        }

        private SubjectData ProcessSubject(ImportMessage message)
        {
            var subjectData = _fileStorageService.GetSubjectData(message).Result;
            var subject =_releaseProcessorService.CreateOrUpdateRelease(subjectData, message);
            
            _context.SaveChanges();
            
            var batches = SplitFileService.GetNumBatches(subjectData.GetCsvLines().Count());
            
            _batchService.UpdateStatus(message.Release.Id.ToString(), subject.Id.ToString(), batches, ImportStatus.RUNNING);

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