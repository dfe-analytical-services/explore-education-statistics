using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
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

        public async Task SplitDataFileIfRequired(Guid importId)
        {
            var import = await _dataImportService.GetImport(importId);
            var dataFileStreamProvider = () => _blobStorageService.StreamBlob(PrivateReleaseFiles, import.File.Path());
            
            if (!import.BatchingRequired())
            {
                _logger.LogInformation($"No splitting of datafile: {import.File.Filename} was necessary");
                return;
            }

            _logger.LogInformation($"Splitting Datafile: {import.File.Filename}");
            await SplitFiles(import, dataFileStreamProvider);
            _logger.LogInformation($"Split of Datafile: {import.File.Filename} complete");
        }

        public async Task AddBatchDataFileMessages(
            Guid importId,
            ICollector<ImportObservationsMessage> collector)
        {
            var import = await _dataImportService.GetImport(importId);

            // If no batching was necessary, simply add a message to process the lone data file
            if (!import.BatchingRequired())
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
            var batchFilesForDataFile = await _batchService.GetBatchFilesForDataFile(import.File);

            // If batching was required but no batch files remain to process, the import has been completed but
            // cut off prior to doing the last status update at Stage 4.  Therefore set the status to complete.
            if (!batchFilesForDataFile.Any())
            {
                await _dataImportService.UpdateStatus(importId, DataImportStatus.COMPLETE, 100);
            }
            
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
            }).ToList();

            importBatchFileMessages.ForEach(collector.Add);
        }

        private async Task SplitFiles(
            DataImport dataImport,
            Func<Task<Stream>> dataFileStreamProvider)
        {
            var csvHeaders = await CsvUtils.GetCsvHeaders(dataFileStreamProvider);

            var existingBatchFiles = await _batchService.GetBatchFilesForDataFile(dataImport.File);

            var existingBatchFileNumbers = existingBatchFiles
                .AsQueryable()
                .Select(blobInfo => GetBatchNumberFromBatchFileName(blobInfo.FileName));

            var streamReader = new StreamReader(await dataFileStreamProvider.Invoke());
            await streamReader.ReadLineAsync();
            
            var currentBatchNumber = 1;
            
            while (currentBatchNumber <= dataImport.NumBatches)
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

                while (!streamReader.EndOfStream && currentRowNumberInBatch <= dataImport.RowsPerBatch)
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
                
                var percentageComplete = (double) currentBatchNumber / dataImport.NumBatches * 100;

                await _dataImportService.UpdateStatus(dataImport.Id, DataImportStatus.STAGE_3, percentageComplete);

                currentBatchNumber++;
            }
        }

        private async Task SkipToNextBatch(DataImport dataImport, StreamReader streamReader)
        {
            var currentRowNumberInBatch = 1;

            while (!streamReader.EndOfStream && currentRowNumberInBatch <= dataImport.RowsPerBatch)
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
