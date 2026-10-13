#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Storage.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services;

public class ProcessorService(
    ILogger<ProcessorService> logger,
    IPrivateBlobStorageService privateBlobStorageService,
    IFileImportService fileImportService,
    IImporterService importerService,
    IDataImportService dataImportService,
    IValidatorService validatorService,
    IStorageDataSetResolver storageDataSetResolver,
    IStorageDataSetWriterResolver storageDataSetWriterResolver
) : IProcessorService
{
    public async Task ProcessStage1(Guid importId)
    {
        await validatorService
            .Validate(importId)
            .OnSuccessDo(async result =>
            {
                await dataImportService.Update(
                    importId,
                    expectedImportedRows: result.ImportableRowCount,
                    totalRows: result.TotalRowCount,
                    geographicLevels: result.GeographicLevels
                );
            })
            .OnFailureDo(async errors =>
            {
                await dataImportService.FailImport(importId, errors);

                logger.LogError("Import {ImportId} FAILED ...check log", importId);
            });
    }

    public async Task ProcessStage2(Guid importId)
    {
        var import = await dataImportService.GetImport(importId);

        var dataSet = storageDataSetResolver.Resolve(import.File);
        var dataSetWriter = storageDataSetWriterResolver.Resolve(import.File);

        var metaFileStreamProvider = privateBlobStorageService.GetMetadataFileStreamProvider(import);

        var metaFileCsvHeaders = await CsvUtils.GetCsvHeaders(metaFileStreamProvider);
        var metaFileCsvRows = await CsvUtils.GetCsvRows(metaFileStreamProvider);

        var subjectMeta = await importerService.ImportMeta(metaFileCsvHeaders, metaFileCsvRows, dataSet, dataSetWriter);
        await fileImportService.ImportFiltersAndLocations(import.Id, subjectMeta, dataSetWriter);
    }

    public async Task ProcessStage3(Guid importId)
    {
        var import = await dataImportService.GetImport(importId);

        try
        {
            var dataSet = storageDataSetResolver.Resolve(import.File);
            var dataSetWriter = storageDataSetWriterResolver.Resolve(import.File);
            await fileImportService.ImportObservations(import, dataSet, dataSetWriter);

            var completedImport = await dataImportService.GetImport(import.Id);
            await fileImportService.CompleteImport(completedImport, dataSet);
        }
        catch (Exception e)
        {
            // If deadlock exception then throw & try up to 3 times
            if (e is SqlException exception && exception.Number == 1205)
            {
                logger.LogInformation(
                    "ProcessStage3: Handling known exception when processing Import "
                        + "{ImportId}: {Message} : transaction will be retried",
                    import.Id,
                    exception.Message
                );
                throw;
            }

            var mainException = e.InnerException ?? e;

            logger.LogError(
                mainException,
                "ProcessStage3 FAILED for Import: {ImportId} : {Message}",
                import.Id,
                mainException.Message
            );

            await dataImportService.FailImport(import.Id);
        }
    }
}
