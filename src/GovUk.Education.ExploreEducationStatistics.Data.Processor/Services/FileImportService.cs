using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Data.Importer.Exceptions;
using GovUk.Education.ExploreEducationStatistics.Data.Importer.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services
{
    public class FileImportService : IFileImportService
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileStorageService _fileStorageService;
        private readonly IImporterService _importerService;
        private readonly IBatchService _batchService;
        private readonly ILogger<IFileImportService> _logger;

        public FileImportService(
            ApplicationDbContext context,
            IFileStorageService fileStorageService,
            IImporterService importerService,
            IBatchService batchService,
            ILogger<IFileImportService> logger)
        {
            _context = context;
            _fileStorageService = fileStorageService;
            _importerService = importerService;
            _batchService = batchService;
            _logger = logger;
        }

        public async Task ImportObservations(ImportMessage message)
        {
            var subjectData = _fileStorageService.GetSubjectData(message).Result;
            var subject = GetSubject(message, subjectData.Name);

            try
            {
                _batchService.UpdateStatus(message.Release.Id.ToString(), subject.Id.ToString(), ImportStatus.RUNNING_PHASE_2);

                var batch = subjectData.GetCsvLines().ToList();
                var metaLines = subjectData.GetMetaLines().ToList();

                _logger.LogInformation($"Start import of observations for {message.DataFileName}");  
                
                _importerService.ImportObservations(
                    batch,
                    subject,
                    _importerService.GetMeta(metaLines, subject));

                // If the batch size is > 1 i.e. The file was split into batches
                // then delete each split batch processed
                    
                if (message.BatchSize > 1)
                {
                    _fileStorageService.Delete(message);
                }
                
                _batchService.UpdateBatchCount(
                    message.Release.Id.ToString(), subject.Id.ToString(), message.BatchSize, message.BatchNo).Wait();
            }
            catch (Exception e)
            {
                _batchService.LogErrors(
                    message.Release.Id.ToString(),
                    subject.Id.ToString(),
                    new List<String>{e.Message},
                    message.BatchNo).Wait();
                
                _logger.LogError(
                    $"{GetType().Name} function FAILED: : Batch: " +
                    $"{message.BatchNo} of {message.BatchSize} with Datafile: " +
                    $"{message.DataFileName} : {e.Message} : will retry unknown exceptions 3 times...");
                
                // If it's a unknown exception then might be an sql deadlock so
                // log error but allow retry upto 3 times
                if (e is ImporterException ex)
                {
                    return;
                }
                throw e;
            }
        }
        
        public void ImportFiltersLocationsAndSchools(ImportMessage message)
        {
            var subjectData = _fileStorageService.GetSubjectData(message).Result;
            var batch = subjectData.GetCsvLines().ToList();
            var metaLines = subjectData.GetMetaLines().ToList();
            var subject = GetSubject(message, subjectData.Name);
            
            _importerService.ImportFiltersLocationsAndSchools(
                batch,
                _importerService.GetMeta(metaLines, subject),
                subject.Name);
        }

        private Subject GetSubject(ImportMessage message, string subjectName)
        {
            return _context.Subject
                .Include(s => s.Release)
                .ThenInclude(r => r.Publication)
                .ThenInclude(p => p.Topic)
                .ThenInclude(t => t.Theme)
                .FirstOrDefault(s => s.Name.Equals(subjectName) && s.ReleaseId == message.Release.Id);
        }
    }
}