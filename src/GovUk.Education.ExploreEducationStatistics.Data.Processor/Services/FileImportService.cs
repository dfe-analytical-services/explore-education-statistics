using System;
using System.Linq;
using System.Threading.Tasks;
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
            
            await _batchService.UpdateBatchCount(message.Release.Id.ToString(), subject.Id.ToString(), message.BatchSize, message.BatchNo);

            var batchComplete = await _batchService.IsBatchComplete(message.Release.Id.ToString(), subject.Id.ToString(), message.BatchSize);
            
            if (batchComplete)
            {
                _logger.LogInformation($"All batches imported for {message.DataFileName}"); 
                await _batchService.UpdateStatus(message.Release.Id.ToString(), subject.Id.ToString(), message.BatchSize, ImportStatus.COMPLETE);
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