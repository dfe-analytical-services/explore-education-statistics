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
        private readonly IBatchService _batchService;
        private readonly IFileStorageService _fileStorageService;
        private readonly IImporterService _importerService;
        private readonly ILogger<IFileImportService> _logger;

        public FileImportService(
            IFileStorageService fileStorageService,
            IImporterService importerService,
            IBatchService batchService,
            ILogger<IFileImportService> logger)
        {
            _fileStorageService = fileStorageService;
            _importerService = importerService;
            _batchService = batchService;
            _logger = logger;
        }

        public async Task ImportObservations(ImportMessage message, StatisticsDbContext context)
        {
            var subjectData = await _fileStorageService.GetSubjectData(message);
            var subject = GetSubject(message, subjectData.Name, context);
            var releaseId = message.Release.Id.ToString();

            if (await _batchService.IsBatchProcessed(releaseId, message.OrigDataFileName, message.BatchNo))
            {
                _logger.LogInformation($"{message.DataFileName} already processed...skipping");
                return;
            }

            // Potentially status could already be failed so don't continue
            if (await _batchService.UpdateStatus(message.Release.Id.ToString(), message.OrigDataFileName,
                IStatus.RUNNING_PHASE_3))
                try
                {
                    _importerService.ImportObservations(
                        subjectData.GetCsvLines().ToList(),
                        subject,
                        _importerService.GetMeta(subjectData.GetMetaLines().ToList(), subject, context),
                        message.BatchNo,
                        message.RowsPerBatch,
                        context
                    );

                    // If the batch size is > 1 i.e. The file was split into batches
                    // then delete each split batch processed

                    if (message.NumBatches > 1)
                        _fileStorageService.Delete(message.Release.Id.ToString(), message.DataFileName);

                    await _batchService.UpdateBatchCount(
                        message.Release.Id.ToString(), message.OrigDataFileName, message.BatchNo);
                }
                catch (Exception e)
                {
                    await _batchService.LogErrors(
                        message.Release.Id.ToString(),
                        message.OrigDataFileName,
                        new List<string> {e.Message}
                    );

                    _logger.LogError(
                        $"{GetType().Name} function FAILED: : Batch: " +
                        $"{message.BatchNo} of {message.NumBatches} with Datafile: " +
                        $"{message.DataFileName} : {e.Message} : will retry unknown exceptions 3 times...");

                    throw e;
                }
        }

        public void ImportFiltersLocationsAndSchools(ImportMessage message, StatisticsDbContext context)
        {
            var subjectData = _fileStorageService.GetSubjectData(message).Result;
            var batch = subjectData.GetCsvLines().ToList();
            var metaLines = subjectData.GetMetaLines().ToList();
            var subject = GetSubject(message, subjectData.Name, context);

            _importerService.ImportFiltersLocationsAndSchools(
                batch,
                _importerService.GetMeta(metaLines, subject, context),
                subject,
                context);
        }

        private static Subject GetSubject(ImportMessage message, string subjectName, StatisticsDbContext context)
        {
            return context.Subject
                .Include(s => s.Release)
                .ThenInclude(r => r.Publication)
                .ThenInclude(p => p.Topic)
                .ThenInclude(t => t.Theme)
                .FirstOrDefault(s => s.Name.Equals(subjectName) && s.ReleaseId == message.Release.Id);
        }
    }
}