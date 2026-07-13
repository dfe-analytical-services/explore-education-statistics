#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.DataImportStatus;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services;

public class FileImportService(
    ILogger<FileImportService> logger,
    IPrivateBlobStorageService privateBlobStorageService,
    IDataImportService dataImportService,
    IDataSetMappingService dataSetMappingService,
    IImporterService importerService
) : IFileImportService
{
    public async Task ImportObservations(DataImport import, StatisticsDbContext context)
    {
        logger.LogInformation("Importing Observations for {Filename}", import.File.Filename);

        if (import.Status.IsFinished())
        {
            logger.LogInformation(
                "Import for {Filename} already finished with state {ImportStatus} - ignoring Observations",
                import.File.Filename,
                import.Status
            );

            return;
        }

        if (import.Status == CANCELLING)
        {
            logger.LogInformation(
                "Import for {Filename} is {ImportStatus} - ignoring Observations and marking import as " + "CANCELLED",
                import.File.Filename,
                import.Status
            );

            await dataImportService.UpdateStatus(import.Id, CANCELLED, 100);
            return;
        }

        var subject = await context.Subject.SingleAsync(s => s.Id.Equals(import.SubjectId));

        var datafileStreamProvider = privateBlobStorageService.GetDataFileStreamProvider(import);
        var metaFileStreamProvider = privateBlobStorageService.GetMetadataFileStreamProvider(import);

        await importerService.ImportObservations(
            import,
            datafileStreamProvider,
            metaFileStreamProvider,
            subject,
            context
        );
    }

    public async Task ImportFiltersAndLocations(Guid importId, SubjectMeta subjectMeta, StatisticsDbContext context)
    {
        var import = await dataImportService.GetImport(importId);

        var datafileStreamProvider = privateBlobStorageService.GetDataFileStreamProvider(import);

        await importerService.ImportFiltersAndLocations(import, datafileStreamProvider, subjectMeta, context);
    }

    public async Task CompleteImport(DataImport import, StatisticsDbContext context)
    {
        if (import.Status.IsFinished())
        {
            logger.LogInformation(
                "Import for {Filename} is already finished in state {ImportStatus} - not attempting to "
                    + "mark as completed or failed",
                import.File.Filename,
                import.Status
            );
            return;
        }

        if (import.Status.IsAborting())
        {
            logger.LogInformation(
                "Import for {Filename} is trying to abort in "
                    + "state {ImportStatus} - not attempting to mark as completed or failed, but "
                    + "instead marking as {AbortedState}, the final state of the aborting process",
                import.File.Filename,
                import.Status,
                import.Status.GetFinishingStateOfAbortProcess()
            );

            await dataImportService.UpdateStatus(import.Id, import.Status.GetFinishingStateOfAbortProcess(), 100);
            return;
        }

        var observationCount = await context.Observation.CountAsync(o => o.SubjectId.Equals(import.SubjectId));
        if (observationCount != import.ExpectedImportedRows)
        {
            await dataImportService.FailImport(
                import.Id,
                $"Number of observations inserted ({observationCount}) "
                    + $"does not equal that expected ({import.ExpectedImportedRows}) : Please delete & retry"
            );
            return;
        }

        if (import.Errors.Count > 0)
        {
            await dataImportService.UpdateStatus(import.Id, FAILED, 100);
            return;
        }

        await FinalImportTasks(import);
        await dataImportService.UpdateStatus(import.Id, COMPLETE, 100);
    }

    private async Task FinalImportTasks(DataImport import)
    {
        try
        {
            await dataSetMappingService.CreateInitialDataSetMappingIfReplacement(import.FileId);
            await dataImportService.WriteDataSetFileMeta(import.FileId, import.SubjectId, import.TotalRows!.Value);
        }
        catch (Exception e)
        {
            await dataImportService.FailImport(import.Id, $"Failed to complete final import tasks. Exception: {e}");
            throw new Exception($"Failed to complete final import tasks\n{e}");
        }
    }
}
