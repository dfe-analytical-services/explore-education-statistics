using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
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
        private readonly StatisticsDbContext _context;
        private readonly IFileStorageService _fileStorageService;
        private readonly IImporterService _importerService;
        private readonly IBatchService _batchService;
        private readonly ILogger<IFileImportService> _logger;

        public FileImportService(
            StatisticsDbContext context,
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
            var dataFileName = message.NumBatches > 1 ? message.DataFileName.Substring(0, message.DataFileName.LastIndexOf("_")) : message.DataFileName;
            var releaseId = message.Release.Id.ToString();

            if (await _batchService.IsBatchProcessed(releaseId, dataFileName, message.BatchNo))
            {
                _logger.LogInformation($"{message.DataFileName} already processed...skipping");
                return;
            }
            
            _batchService.UpdateStatus(message.Release.Id.ToString(), dataFileName, IStatus.RUNNING_PHASE_2);
            
            try
            {
                var batch = subjectData.GetCsvLines().ToList();
                var metaLines = subjectData.GetMetaLines().ToList();
                
                _importerService.ImportObservations(
                    batch,
                    subject,
                    _importerService.GetMeta(metaLines, subject),
                    message.BatchNo,
                    message.RowsPerBatch
                    );

                // If the batch size is > 1 i.e. The file was split into batches
                // then delete each split batch processed
                    
                if (message.NumBatches > 1)
                {
                    _fileStorageService.Delete(message.Release.Id.ToString(), message.DataFileName);
                }
                
                _batchService.UpdateBatchCount(
                    message.Release.Id.ToString(), dataFileName, message.BatchNo).Wait();
            }
            catch (Exception e)
            {
                _batchService.LogErrors(
                    message.Release.Id.ToString(),
                    dataFileName,
                    new List<String>{e.Message}
                    ).Wait();
                
                _logger.LogError(
                    $"{GetType().Name} function FAILED: : Batch: " +
                    $"{message.BatchNo} of {message.NumBatches} with Datafile: " +
                    $"{message.DataFileName} : {e.Message} : will retry unknown exceptions 3 times...");
                
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
                subject);
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