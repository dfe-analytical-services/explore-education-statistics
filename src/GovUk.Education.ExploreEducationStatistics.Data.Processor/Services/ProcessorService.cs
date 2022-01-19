using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Utils;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services
{
    public class ProcessorService : IProcessorService
    {
        private readonly ILogger<ProcessorService> _logger;
        private readonly IBlobStorageService _blobStorageService;
        private readonly IFileImportService _fileImportService;
        private readonly IImporterService _importerService;
        private readonly IDataImportService _dataImportService;
        private readonly ISplitFileService _splitFileService;
        private readonly IValidatorService _validatorService;
        private readonly IDataArchiveService _dataArchiveService;

        public ProcessorService(
            ILogger<ProcessorService> logger,
            IBlobStorageService blobStorageService,
            IFileImportService fileImportService,
            ISplitFileService splitFileService,
            IImporterService importerService,
            IDataImportService dataImportService,
            IValidatorService validatorService,
            IDataArchiveService dataArchiveService)
        {
            _logger = logger;
            _blobStorageService = blobStorageService;
            _fileImportService = fileImportService;
            _splitFileService = splitFileService;
            _importerService = importerService;
            _dataImportService = dataImportService;
            _validatorService = validatorService;
            _dataArchiveService = dataArchiveService;
        }

        public async Task ProcessUnpackingArchive(Guid importId)
        {
            var import = await _dataImportService.GetImport(importId);
            await _dataArchiveService.ExtractDataFiles(import);
        }

        public async Task ProcessStage1(Guid importId, ExecutionContext executionContext)
        {
            await _validatorService.Validate(importId, executionContext)
                .OnSuccessDo(async result =>
                {
                    await _dataImportService.Update(importId, 
                        rowsPerBatch: result.RowsPerBatch,
                        totalRows: result.TotalRows,
                        numBatches: result.NumBatches,
                        geographicLevels: result.GeographicLevels);
                })
                .OnFailureDo(async errors =>
                {
                    await _dataImportService.FailImport(importId, errors);

                    _logger.LogError($"Import {importId} FAILED ...check log");
                });
        }

        public async Task ProcessStage2(Guid importId)
        {
            var statisticsDbContext = DbUtils.CreateStatisticsDbContext();

            var import = await _dataImportService.GetImport(importId);

            var subject = await statisticsDbContext.Subject.FindAsync(import.SubjectId);

            var metaFileStream = await _blobStorageService.StreamBlob(PrivateReleaseFiles, import.MetaFile.Path());
            var metaFileTable = DataTableUtils.CreateFromStream(metaFileStream);

            _importerService.ImportMeta(metaFileTable, subject, statisticsDbContext);
            await statisticsDbContext.SaveChangesAsync();

            await _fileImportService.ImportFiltersAndLocations(import.Id, statisticsDbContext);
            await statisticsDbContext.SaveChangesAsync();
        }

        public async Task ProcessStage3(Guid importId)
        {
            await _splitFileService.SplitDataFile(importId);
        }

        public async Task ProcessStage4Messages(Guid importId, ICollector<ImportObservationsMessage> collector)
        {
            await _splitFileService.AddBatchDataFileMessages(importId, collector);
        }
    }
}
