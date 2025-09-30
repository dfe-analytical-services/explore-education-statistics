#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services;

public class ProcessorService : IProcessorService
{
    private readonly ILogger<ProcessorService> _logger;
    private readonly IPrivateBlobStorageService _privateBlobStorageService;
    private readonly IFileImportService _fileImportService;
    private readonly IImporterService _importerService;
    private readonly IDataImportService _dataImportService;
    private readonly IValidatorService _validatorService;
    private readonly IDbContextSupplier _dbContextSupplier;

    public ProcessorService(
        ILogger<ProcessorService> logger,
        IPrivateBlobStorageService privateBlobStorageService,
        IFileImportService fileImportService,
        IImporterService importerService,
        IDataImportService dataImportService,
        IValidatorService validatorService,
        IDbContextSupplier dbContextSupplier
    )
    {
        _logger = logger;
        _privateBlobStorageService = privateBlobStorageService;
        _fileImportService = fileImportService;
        _importerService = importerService;
        _dataImportService = dataImportService;
        _validatorService = validatorService;
        _dbContextSupplier = dbContextSupplier;
    }

    public async Task ProcessStage1(Guid importId)
    {
        await _validatorService
            .Validate(importId)
            .OnSuccessDo(async result =>
            {
                await _dataImportService.Update(
                    importId,
                    expectedImportedRows: result.ImportableRowCount,
                    totalRows: result.TotalRowCount,
                    geographicLevels: result.GeographicLevels
                );
            })
            .OnFailureDo(async errors =>
            {
                await _dataImportService.FailImport(importId, errors);

                _logger.LogError("Import {ImportId} FAILED ...check log", importId);
            });
    }

    public async Task ProcessStage2(Guid importId)
    {
        var statisticsDbContext = _dbContextSupplier.CreateDbContext<StatisticsDbContext>();

        var import = await _dataImportService.GetImport(importId);

        var subject = await statisticsDbContext.Subject.SingleAsync(subject =>
            subject.Id == import.SubjectId
        );

        var metaFileStreamProvider = _privateBlobStorageService.GetMetadataFileStreamProvider(
            import
        );

        var metaFileCsvHeaders = await CsvUtils.GetCsvHeaders(metaFileStreamProvider);
        var metaFileCsvRows = await CsvUtils.GetCsvRows(metaFileStreamProvider);

        var subjectMeta = await _importerService.ImportMeta(
            metaFileCsvHeaders,
            metaFileCsvRows,
            subject,
            statisticsDbContext
        );
        await _fileImportService.ImportFiltersAndLocations(
            import.Id,
            subjectMeta,
            statisticsDbContext
        );
    }

    public async Task ProcessStage3(Guid importId)
    {
        var import = await _dataImportService.GetImport(importId);

        try
        {
            await _fileImportService.ImportObservations(
                import,
                _dbContextSupplier.CreateDbContext<StatisticsDbContext>()
            );
        }
        catch (Exception e)
        {
            // If deadlock exception then throw & try up to 3 times
            if (e is SqlException exception && exception.Number == 1205)
            {
                _logger.LogInformation(
                    "ProcessStage3: Handling known exception when processing Import "
                        + "{ImportId}: {Message} : transaction will be retried",
                    import.Id,
                    exception.Message
                );
                throw;
            }

            var mainException = e.InnerException ?? e;

            _logger.LogError(
                mainException,
                "ProcessStage3 FAILED for Import: {ImportId} : {Message}",
                import.Id,
                mainException.Message
            );

            await _dataImportService.FailImport(import.Id);
        }
    }
}
