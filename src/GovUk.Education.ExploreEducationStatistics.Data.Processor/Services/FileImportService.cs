# nullable enable
using System;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.DataImportStatus;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services
{
    public class FileImportService : IFileImportService
    {
        private readonly ILogger<FileImportService> _logger;
        private readonly IBlobStorageService _blobStorageService;
        private readonly IDataImportService _dataImportService;
        private readonly IImporterService _importerService;

        public FileImportService(
            ILogger<FileImportService> logger,
            IBlobStorageService blobStorageService,
            IDataImportService dataImportService,
            IImporterService importerService)
        {
            _logger = logger;
            _blobStorageService = blobStorageService;
            _dataImportService = dataImportService;
            _importerService = importerService;
        }

        public async Task ImportObservations(DataImport import, StatisticsDbContext context)
        {
            _logger.LogInformation("Importing Observations for {Filename}", import.File.Filename);

            if (import.Status.IsFinished())
            {
                _logger.LogInformation(
                    "Import for {Filename} already finished with state {ImportStatus} - ignoring Observations",
                    import.File.Filename,
                    import.Status
                );
                
                return;
            }

            if (import.Status == CANCELLING)
            {
                _logger.LogInformation(
                    "Import for {Filename} is {ImportStatus} - ignoring Observations and marking import as " +
                    "CANCELLED",
                    import.File.Filename,
                    import.Status);

                await _dataImportService.UpdateStatus(import.Id, CANCELLED, 100);
                return;
            }

            var subject = await context.Subject.FindAsync(import.SubjectId);

            var datafileStreamProvider = () => _blobStorageService.StreamBlob(PrivateReleaseFiles, import.File.Path());
            var metaFileStreamProvider = () => _blobStorageService.StreamBlob(PrivateReleaseFiles, import.MetaFile.Path());

            var metaFileCsvHeaders = await CsvUtils.GetCsvHeaders(metaFileStreamProvider);
            var metaFileCsvRows = await CsvUtils.GetCsvRows(metaFileStreamProvider);

            await _importerService.ImportObservations(
                import,
                datafileStreamProvider,
                subject!,
                _importerService.GetMeta(metaFileCsvHeaders, metaFileCsvRows, subject!, context),
                context
            );

            var completedImport = await _dataImportService.GetImport(import.Id);
            await CheckComplete(completedImport, context);
        }

        public async Task ImportFiltersAndLocations(Guid importId, StatisticsDbContext context)
        {
            var import = await _dataImportService.GetImport(importId);

            var subject = await context.Subject.FindAsync(import.SubjectId);

            var datafileStreamProvider = () => _blobStorageService.StreamBlob(PrivateReleaseFiles, import.File.Path());
            var metaFileStreamProvider = () => _blobStorageService.StreamBlob(PrivateReleaseFiles, import.MetaFile.Path());

            var metaFileCsvHeaders = await CsvUtils.GetCsvHeaders(metaFileStreamProvider);
            var metaFileCsvRows = await CsvUtils.GetCsvRows(metaFileStreamProvider);

            await _importerService.ImportFiltersAndLocations(
                import,
                datafileStreamProvider,
                _importerService.GetMeta(metaFileCsvHeaders, metaFileCsvRows, subject!, context),
                context);
        }

        public async Task CheckComplete(DataImport import, StatisticsDbContext context)
        {
            if (import.Status.IsFinished())
            {
                _logger.LogInformation(
                    "Import for {Filename} is already finished in state {ImportStatus} - not attempting to " +
                    "mark as completed or failed",
                    import.File.Filename,
                    import.Status);
                return;
            }

            if (import.Status.IsAborting())
            {
                _logger.LogInformation("Import for {Filename} is trying to abort in " +
                                       "state {ImportStatus} - not attempting to mark as completed or failed, but " +
                                       "instead marking as {AbortedState}, the final state of the aborting process",
                    import.File.Filename,
                    import.Status,
                    import.Status.GetFinishingStateOfAbortProcess());

                await _dataImportService.UpdateStatus(import.Id, import.Status.GetFinishingStateOfAbortProcess(), 100);
                return;
            }

            var observationCount = context.Observation.Count(o => o.SubjectId.Equals(import.SubjectId));

            if (!observationCount.Equals(import.ExpectedImportedRows))
            {
                await _dataImportService.FailImport(import.Id,
                    $"Number of observations inserted ({observationCount}) " +
                            $"does not equal that expected ({import.ExpectedImportedRows}) : Please delete & retry");
            }
            else
            {
                if (import.Errors.Count == 0)
                {
                    await _dataImportService.UpdateStatus(import.Id, COMPLETE, 100);
                }
                else
                {
                    await _dataImportService.UpdateStatus(import.Id, FAILED, 100);
                }
            }
        }
    }
}
