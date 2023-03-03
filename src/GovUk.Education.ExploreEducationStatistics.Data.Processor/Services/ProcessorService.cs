#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services
{
    public class ProcessorService : IProcessorService
    {
        private readonly IBlobStorageService _blobStorageService;
        private readonly IFileImportService _fileImportService;
        private readonly IImporterService _importerService;
        private readonly IDataImportService _dataImportService;
        private readonly ISplitFileService _splitFileService;
        private readonly IValidatorService _validatorService;
        private readonly IDataArchiveService _dataArchiveService;
        private readonly IDbContextSupplier _dbContextSupplier;
        private readonly ILogger<ProcessorService> _logger;
        
        public ProcessorService(
            ILogger<ProcessorService> logger,
            IBlobStorageService blobStorageService,
            IFileImportService fileImportService,
            ISplitFileService splitFileService,
            IImporterService importerService,
            IDataImportService dataImportService,
            IValidatorService validatorService,
            IDataArchiveService dataArchiveService,
            IDbContextSupplier dbContextSupplier)
        {
            _logger = logger;
            _blobStorageService = blobStorageService;
            _fileImportService = fileImportService;
            _splitFileService = splitFileService;
            _importerService = importerService;
            _dataImportService = dataImportService;
            _validatorService = validatorService;
            _dataArchiveService = dataArchiveService;
            _dbContextSupplier = dbContextSupplier;
        }

        public async Task ProcessUnpackingArchive(Guid importId)
        {
            var import = await _dataImportService.GetImport(importId);
            await _dataArchiveService.ExtractDataFiles(import);
        }

        public async Task ProcessStage1(Guid importId)
        {
            await _validatorService.Validate(importId)
                .OnSuccessDo(async result =>
                {
                    await _dataImportService.Update(importId,
                        rowsPerBatch: result.RowsPerBatch,
                        importedRows: result.ImportableRowCount,
                        totalRows: result.TotalRowCount,
                        numBatches: result.NumBatches,
                        geographicLevels: result.GeographicLevels);
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

            var subject = await statisticsDbContext.Subject.SingleAsync(subject => subject.Id == import.SubjectId);

            var metaFileStreamProvider = () => _blobStorageService.StreamBlob(PrivateReleaseFiles, import.MetaFile.Path());

            var metaFileCsvHeaders = await CsvUtils.GetCsvHeaders(metaFileStreamProvider);
            var metaFileCsvRows = await CsvUtils.GetCsvRows(metaFileStreamProvider);

            await _importerService.ImportMeta(metaFileCsvHeaders, metaFileCsvRows, subject, statisticsDbContext);
            await _fileImportService.ImportFiltersAndLocations(import.Id, statisticsDbContext);
        }

        public async Task ProcessStage3(Guid importId)
        {
            await _splitFileService.SplitDataFileIfRequired(importId);
        }

        public async Task ProcessStage4Messages(Guid importId, ICollector<ImportObservationsMessage> collector)
        {
            await _splitFileService.AddBatchDataFileMessages(importId, collector);
        }
    }
}
