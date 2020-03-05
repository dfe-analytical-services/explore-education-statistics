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
            var releaseId = message.Release.Id.ToString();

            // Potentially status could already be failed so don't continue
            if (await _batchService.UpdateStatus(releaseId, message.OrigDataFileName,
                IStatus.RUNNING_PHASE_3))
            {
                var subjectData = await _fileStorageService.GetSubjectData(message);
                var subject = GetSubject(message, subjectData.Name, context);

                _importerService.ImportObservations(
                    subjectData.GetCsvLines().ToList(),
                    subject,
                    _importerService.GetMeta(subjectData.GetMetaLines().ToList(), subject, context),
                    message.BatchNo,
                    message.RowsPerBatch,
                    context
                );
                
                await _batchService.CheckComplete(releaseId, message);
            }
            else
            {
                _logger.LogInformation($"{message.DataFileName} already failed...skipping");
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