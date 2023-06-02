# nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.DataImportStatus;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services
{
    public class FileImportService : IFileImportService
    {
        private readonly ILogger<FileImportService> _logger;
        private readonly IPrivateBlobStorageService _privateBlobStorageService;
        private readonly IDataImportService _dataImportService;
        private readonly IImporterService _importerService;

        public FileImportService(
            ILogger<FileImportService> logger,
            IPrivateBlobStorageService privateBlobStorageService,
            IDataImportService dataImportService,
            IImporterService importerService)
        {
            _logger = logger;
            _privateBlobStorageService = privateBlobStorageService;
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

            var subject = await context.Subject.SingleAsync(s => s.Id.Equals(import.SubjectId));

            var datafileStreamProvider = () => _privateBlobStorageService.StreamBlob(PrivateReleaseFiles, import.File.Path());
            var metaFileStreamProvider = () => _privateBlobStorageService.StreamBlob(PrivateReleaseFiles, import.MetaFile.Path());

            await _importerService.ImportObservations(
                import,
                datafileStreamProvider,
                metaFileStreamProvider,
                subject,
                context
            );

            var completedImport = await _dataImportService.GetImport(import.Id);
            await CheckComplete(completedImport, context);
        }

        public async Task ImportFiltersAndLocations(
            Guid importId,
            SubjectMeta subjectMeta,
            StatisticsDbContext context)
        {
            var import = await _dataImportService.GetImport(importId);

            var datafileStreamProvider = () => _privateBlobStorageService.StreamBlob(PrivateReleaseFiles, import.File.Path());

            await _importerService.ImportFiltersAndLocations(
                import,
                datafileStreamProvider,
                subjectMeta,
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

            var observationCount = await context
                .Observation
                .CountAsync(o => o.SubjectId.Equals(import.SubjectId));

            if (observationCount != import.ExpectedImportedRows)
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
