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
                var releaseSubject = GetReleaseSubjectLink(message, subjectData.Name, context);
                var csvTable = subjectData.GetCsvTable();
                var metaTable = subjectData.GetMetaTable();
                
                context.Database.BeginTransaction();
                
                _importerService.ImportObservations(
                    csvTable.Columns,
                    csvTable.Rows,
                    releaseSubject.Subject,
                    _importerService.GetMeta(metaTable, releaseSubject.Subject, context),
                    message.BatchNo,
                    message.RowsPerBatch,
                    context
                );
                
                context.Database.CommitTransaction();
                
                await _batchService.CheckComplete(releaseId, message, context);
            }
            else
            {
                _logger.LogInformation($"{message.DataFileName} already failed...skipping");
            }
        }

        public void ImportFiltersLocationsAndSchools(ImportMessage message, StatisticsDbContext context)
        {
            var subjectData = _fileStorageService.GetSubjectData(message).Result;
            var releaseSubject = GetReleaseSubjectLink(message, subjectData.Name, context);
            var csvTable = subjectData.GetCsvTable();
            var metaTable = subjectData.GetMetaTable();

            _importerService.ImportFiltersLocationsAndSchools(
                csvTable.Columns,
                csvTable.Rows,
                _importerService.GetMeta(metaTable, releaseSubject.Subject, context),
                context);
        }

        private static ReleaseSubject GetReleaseSubjectLink(ImportMessage message, string subjectName, StatisticsDbContext context)
        {
            return context
                .ReleaseSubject
                .Include(r => r.Subject)
                .Include(r => r.Release)
                .ThenInclude(r => r.Publication)
                .ThenInclude(p => p.Topic)
                .ThenInclude(t => t.Theme)
                .FirstOrDefault(r => r.Subject.Name.Equals(subjectName) && r.ReleaseId == message.Release.Id);
        }
    }
}