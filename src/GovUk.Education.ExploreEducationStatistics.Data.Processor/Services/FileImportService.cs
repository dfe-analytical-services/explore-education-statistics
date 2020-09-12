using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Importer.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Utils;
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

                var dataFileTable = DataTableUtils.CreateFromStream(
                    await _fileStorageService.StreamBlob(subjectData.DataBlob)
                );
                var metaFileTable = DataTableUtils.CreateFromStream(
                    await _fileStorageService.StreamBlob(subjectData.MetaBlob)
                );

                context.Database.CreateExecutionStrategy().Execute(() =>
                {
                    using var transaction = context.Database.BeginTransaction();

                    _importerService.ImportObservations(
                        dataFileTable.Columns,
                        dataFileTable.Rows,
                        releaseSubject.Subject,
                        _importerService.GetMeta(metaFileTable, releaseSubject.Subject, context),
                        message.BatchNo,
                        message.RowsPerBatch,
                        context
                    );

                    transaction.Commit();
                });

                await _batchService.CheckComplete(releaseId, message, context);
            }
            else
            {
                _logger.LogInformation($"{message.DataFileName} already failed...skipping");
            }
        }

        public async Task ImportFiltersLocationsAndSchools(ImportMessage message, StatisticsDbContext context)
        {
            var subjectData = _fileStorageService.GetSubjectData(message).Result;
            var releaseSubject = GetReleaseSubjectLink(message, subjectData.Name, context);

            var dataFileTable = DataTableUtils.CreateFromStream(
                await _fileStorageService.StreamBlob(subjectData.DataBlob)
            );
            var metaFileTable = DataTableUtils.CreateFromStream(
                await _fileStorageService.StreamBlob(subjectData.MetaBlob)
            );

            _importerService.ImportFiltersLocationsAndSchools(
                dataFileTable.Columns,
                dataFileTable.Rows,
                _importerService.GetMeta(metaFileTable, releaseSubject.Subject, context),
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