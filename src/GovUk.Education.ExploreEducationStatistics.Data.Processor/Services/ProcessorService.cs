using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Importer.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Importer.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Utils;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services
{
    public class ProcessorService : IProcessorService
    {
        private readonly ILogger<ProcessorService> _logger;
        private readonly IBatchService _batchService;
        private readonly IFileImportService _fileImportService;
        private readonly IFileStorageService _fileStorageService;
        private readonly IImporterService _importerService;
        private readonly IReleaseProcessorService _releaseProcessorService;
        private readonly ISplitFileService _splitFileService;
        private readonly IValidatorService _validatorService;
        private readonly IDataArchiveService _dataArchiveService;

        public ProcessorService(
            ILogger<ProcessorService> logger,
            IFileImportService fileImportService,
            IReleaseProcessorService releaseProcessorService,
            IFileStorageService fileStorageService,
            ISplitFileService splitFileService,
            IImporterService importerService,
            IBatchService batchService,
            IValidatorService validatorService,
            IDataArchiveService dataArchiveService)
        {
            _logger = logger;
            _fileImportService = fileImportService;
            _releaseProcessorService = releaseProcessorService;
            _fileStorageService = fileStorageService;
            _splitFileService = splitFileService;
            _importerService = importerService;
            _batchService = batchService;
            _validatorService = validatorService;
            _dataArchiveService = dataArchiveService;
        }

        public async Task ProcessUnpackingArchive(ImportMessage message)
        {
            await _dataArchiveService.ExtractDataFiles(message.Release.Id, message.ArchiveFileName);
        }

        public async Task ProcessStage1(ImportMessage message, ExecutionContext executionContext)
        {
            var subjectData = await GetSubjectDataFromMainDataFile(message);

            await _validatorService.Validate(subjectData, executionContext, message)
                .OnSuccessDo(async result =>
                {
                    message.RowsPerBatch = result.RowsPerBatch;
                    message.TotalRows = result.FilteredObservationCount;
                    message.NumBatches = result.NumBatches;
                    await _batchService.UpdateStoredMessage(message);
                })
                .OnFailureDo(async errors =>
                {
                    await _batchService.FailImport(message.Release.Id,
                        message.DataFileName,
                        errors);

                    _logger.LogError($"Import FAILED for {message.DataFileName}...check log");
                });
        }

        public async Task ProcessStage2(ImportMessage message)
        {
            var subjectData = await GetSubjectDataFromMainDataFile(message);

            await ProcessSubject(message,
                DbUtils.CreateStatisticsDbContext(),
                DbUtils.CreateContentDbContext(),
                subjectData);

        }

        public async Task ProcessStage3(ImportMessage message)
        {
            var subjectData = await GetSubjectDataFromMainDataFile(message);

            await _splitFileService.SplitDataFile(message, subjectData);
        }

        public async Task ProcessStage4Messages(ImportMessage message, ICollector<ImportObservationsMessage> collector)
        {
            await _splitFileService.AddBatchDataFileMessages(collector, message);
        }
        
        private async Task ProcessSubject(
            ImportMessage message,
            StatisticsDbContext statisticsDbContext,
            ContentDbContext contentDbContext,
            SubjectData subjectData)
        {
            var subject = _releaseProcessorService.CreateOrUpdateRelease(subjectData,
                message,
                statisticsDbContext,
                contentDbContext);

            await using var metaFileStream = await _fileStorageService.StreamBlob(subjectData.MetaBlob);
            var metaFileTable = DataTableUtils.CreateFromStream(metaFileStream);

            _importerService.ImportMeta(metaFileTable, subject, statisticsDbContext);

            await statisticsDbContext.SaveChangesAsync();

            await _fileImportService.ImportFiltersLocationsAndSchools(message, statisticsDbContext);

            await statisticsDbContext.SaveChangesAsync();
        }

        private async Task<SubjectData> GetSubjectDataFromMainDataFile(ImportMessage message)
        {
            var dataFileBlobPath = AdminReleasePath(message.Release.Id, ReleaseFileTypes.Data, message.DataFileName);

            var subjectData = await _fileStorageService.GetSubjectData(message.Release.Id, dataFileBlobPath);
            return subjectData;
        }
    }
}