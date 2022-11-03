using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Utils;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services
{
    public class SplitFileService : ISplitFileService
    {
        private readonly IBatchService _batchService;
        private readonly IBlobStorageService _blobStorageService;
        private readonly ILogger<SplitFileService> _logger;
        private readonly IDataImportService _dataImportService;

        public SplitFileService(
            IBatchService batchService,
            IBlobStorageService blobStorageService,
            ILogger<SplitFileService> logger,
            IDataImportService dataImportService
            )
        {
            _batchService = batchService;
            _blobStorageService = blobStorageService;
            _logger = logger;
            _dataImportService = dataImportService;
        }

        public async Task SplitDataFile(Guid importId)
        {
            var import = await _dataImportService.GetImport(importId);
            var dataFileStreamProvider = () => _blobStorageService.StreamBlob(PrivateReleaseFiles, import.File.Path());
            var totalRows = await CsvUtil.GetTotalRows(dataFileStreamProvider);
            if (totalRows > import.RowsPerBatch)
            {
                _logger.LogInformation($"Splitting Datafile: {import.File.Filename}");
                await SplitFiles(import, totalRows, dataFileStreamProvider);
                _logger.LogInformation($"Split of Datafile: {import.File.Filename} complete");
            }
            else
            {
                _logger.LogInformation($"No splitting of datafile: {import.File.Filename} was necessary");
            }
        }

        public async Task AddBatchDataFileMessages(
            Guid importId,
            ICollector<ImportObservationsMessage> collector)
        {
            var import = await _dataImportService.GetImport(importId);

            var batchFilesForDataFile = await _batchService.GetBatchFilesForDataFile(import.File);

            // If no batching was necessary, simply add a message to process the lone data file
            if (!batchFilesForDataFile.Any())
            {
                collector.Add(new ImportObservationsMessage
                {
                    Id = import.Id,
                    BatchNo = 1,
                    ObservationsFilePath = import.File.Path()
                });
                return;
            }

            // Otherwise create a message per batch file to process
            var importBatchFileMessages = batchFilesForDataFile.Select(blobInfo =>
            {
                var batchFileName = blobInfo.FileName;
                var batchFilePath = blobInfo.Path;
                var batchNo = GetBatchNumberFromBatchFileName(batchFileName);
                
                return new ImportObservationsMessage
                {
                    Id = import.Id,
                    BatchNo = batchNo,
                    ObservationsFilePath = batchFilePath
                };
            });

            foreach (var importMessage in importBatchFileMessages)
            {
                collector.Add(importMessage);
            }
        }

        private async Task SplitFiles(
            DataImport dataImport,
            int totalRows,
            Func<Task<Stream>> dataFileStreamProvider)
        {
            var csvHeaders = await CsvUtil.GetCsvHeaders(dataFileStreamProvider);
            var totalNumberOfBatches = (int) Math.Ceiling((double) totalRows / dataImport.RowsPerBatch);

            var existingBatchFiles = await _batchService.GetBatchFilesForDataFile(dataImport.File);

            var existingBatchFileNumbers = existingBatchFiles
                .AsQueryable()
                .Select(blobInfo => GetBatchNumberFromBatchFileName(blobInfo.FileName));

            var streamReader = new StreamReader(await dataFileStreamProvider.Invoke());
            await streamReader.ReadLineAsync();
            
            var currentBatchNumber = 1;
            
            while (currentBatchNumber <= totalNumberOfBatches)
            {
                var currentStatus = await _dataImportService.GetImportStatus(dataImport.Id);

                if (currentStatus.IsFinishedOrAborting())
                {
                    _logger.LogInformation(
                        $"Import for {dataImport.File.Filename} is finished or aborting - stopping creating batch files");
                    return;
                }
                
                if (existingBatchFileNumbers.Contains(currentBatchNumber))
                {
                    _logger.LogInformation($"Batch file ${currentBatchNumber} already exists - skipping creating it again");

                    await SkipToNextBatch(dataImport, streamReader);
                    currentBatchNumber++;
                    continue;
                }
                
                await using var writeStream = new MemoryStream();
                var writer = new StreamWriter(writeStream);
                await writer.WriteLineAsync(csvHeaders.JoinToString(','));
                
                var currentRowNumberInBatch = 1;

                while (!streamReader.EndOfStream && currentRowNumberInBatch < dataImport.RowsPerBatch)
                {
                    var nextLine = await streamReader.ReadLineAsync();
                    await writer.WriteLineAsync(nextLine);
                    currentRowNumberInBatch++;
                }
                
                await writer.FlushAsync();
                    
                await _blobStorageService.UploadStream(
                    containerName: PrivateReleaseFiles,
                    path: dataImport.File.BatchPath(currentBatchNumber),
                    stream: writeStream,
                    contentType: "text/csv");
                
                var percentageComplete = (double) currentBatchNumber / totalNumberOfBatches * 100;

                await _dataImportService.UpdateStatus(dataImport.Id, DataImportStatus.STAGE_3, percentageComplete);

                currentBatchNumber++;
            }
        }

        private async Task SkipToNextBatch(DataImport dataImport, StreamReader streamReader)
        {
            var currentRowNumberInBatch = 1;

            while (!streamReader.EndOfStream && currentRowNumberInBatch < dataImport.RowsPerBatch)
            {
                await streamReader.ReadLineAsync();
                currentRowNumberInBatch++;
            }
        }

        private static int GetBatchNumberFromBatchFileName(string batchFileName)
        {
            return Int32.Parse(batchFileName.Substring(batchFileName.LastIndexOf("_", StringComparison.Ordinal) + 1));
        }
    }
}
