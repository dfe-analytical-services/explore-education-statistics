using System;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Importer.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
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
        // ReSharper disable once UnusedMember.Global
        public void ProcessUploads(
            [QueueTrigger("imports-pending", Connection = "")]
            ImportMessage message,
            ILogger logger,
            [Queue("imports-available")] ICollector<ImportMessage> collector
            )
        {
            try
            {
                logger.LogInformation($"{GetType().Name} function STARTED for : Datafile: {message.DataFileName}");
                
                var subjectData = _fileStorageService.GetSubjectData(message).Result;
                
                var subject =_releaseProcessorService.CreateOrUpdateRelease(subjectData, message);
                
                // is this a new subject?
                if (subject.Id <= 0)
                {
                    _context.SaveChanges();

                    _importerService.ImportMeta(subjectData.GetMetaLines().ToList(), subject);
                    
                    _fileImportService.ImportFilters(message);

                    _context.SaveChanges();
                }

                _splitFileService.SplitDataFile(collector, message, subjectData);
                
            }
            catch (Exception e)
            {
                logger.LogError($"{GetType().Name} function FAILED for : Datafile: {message.DataFileName}\n{e}");
                throw;
            }

            logger.LogInformation($"{GetType().Name} function COMPLETE for : Datafile: {message.DataFileName}");
        }
        
        [FunctionName("ImportFiles")]
        // ReSharper disable once UnusedMember.Global
        public void ImportFiles(
            [QueueTrigger("imports-available", Connection = "")]
            ImportMessage message,
            ILogger logger)
        {
            try
            {
                logger.LogInformation($"{GetType().Name} function STARTED for : Batch: {message.BatchNo} of {message.BatchSize} with Datafile: {message.DataFileName}");
                
                _fileImportService.ImportFiles(message);

                // If the batch size is > 1 i.e. The file was split into batches
                // then delete each split batch processed
                
                if (message.BatchSize > 1)
                {
                    _fileStorageService.Delete(message);
                }
            }
            catch (Exception e)
            {
                logger.LogError($"{GetType().Name} function FAILED: : Batch: {message.BatchNo} of {message.BatchSize} with Datafile: {message.DataFileName}\n{e}");
                throw;
            }

            logger.LogInformation($"{GetType().Name} function COMPLETE for : Batch: {message.BatchNo}  of {message.BatchSize} with Datafile: {message.DataFileName}");
        }
    }
}