using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Exceptions;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Utils;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainerNames;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services
{
    public class SplitFileService : ISplitFileService
    {
        private readonly IBatchService _batchService;
        private readonly IBlobStorageService _blobStorageService;
        private readonly ILogger<ISplitFileService> _logger;
        private readonly IDataImportService _dataImportService;

        private static readonly List<GeographicLevel> IgnoredGeographicLevels = new List<GeographicLevel>
        {
            GeographicLevel.Institution,
            GeographicLevel.Provider,
            GeographicLevel.School,
            GeographicLevel.PlanningArea
        };

        public SplitFileService(
            IBatchService batchService,
            IBlobStorageService blobStorageService,
            ILogger<ISplitFileService> logger,
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
            var dataFileStream = await _blobStorageService.StreamBlob(PrivateFilesContainerName, import.File.Path());

            var dataFileTable = DataTableUtils.CreateFromStream(dataFileStream);

            if (dataFileTable.Rows.Count > import.RowsPerBatch)
            {
                _logger.LogInformation($"Splitting Datafile: {import.File.Filename}");
                await SplitFiles(import, dataFileTable);
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

            var batchFilesForDataFile = await _batchService.GetBatchFilesForDataFile(import.FileId);

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
            DataTable dataFileTable)
        {
            var headerList = CsvUtil.GetColumnValues(dataFileTable.Columns);
            var batches = dataFileTable.Rows.OfType<DataRow>().Batch(dataImport.RowsPerBatch);
            var batchCount = 1;
            var numRows = dataFileTable.Rows.Count + 1;
            var numBatches = (int)Math.Ceiling((double)dataFileTable.Rows.Count / dataImport.RowsPerBatch);

            var existingBatchFiles = await _batchService.GetBatchFilesForDataFile(dataImport.FileId);

            var existingBatchFileNumbers = existingBatchFiles
                .AsQueryable()
                .Select(blobInfo => GetBatchNumberFromBatchFileName(blobInfo.FileName));

            // TODO: EES-1608 - this flag keeps a track of whether any batch files have been generated to date.
            // It is used in a legacy check to determine whether or not to generate a "no rows" batch file.
            // EES-1608 will investigate what the circumstances are that could lead to a "no rows" batch file
            // situation, and whether this check can actually be entirely removed or not.
            var batchFilesExist = existingBatchFileNumbers.Any();

            foreach (var batch in batches)
            {
                var currentStatus = await _dataImportService.GetImportStatus(dataImport.Id);

                if (currentStatus.IsFinishedOrAborting())
                {
                    _logger.LogInformation(
                        $"Import for {dataImport.File.Filename} is finished or aborting - stopping creating batch files");
                    return;
                }

                var batchFileName = $"{dataImport.File.Filename}_{batchCount:000000}";

                if (existingBatchFileNumbers.Contains(batchCount))
                {
                    _logger.LogInformation($"Batch file {batchFileName} already exists - not recreating");
                    batchCount++;
                    continue;    
                }

                var batchFilePath = $"{BatchesDir}/{batchFileName}";

                await using var stream = new MemoryStream();
                var writer = new StreamWriter(stream);
                await writer.FlushAsync();

                var table = new DataTable();
                CopyColumns(dataFileTable, table);
                CopyRows(table, batch.ToList(), headerList);

                var percentageComplete = (double) batchCount / numBatches * 100;

                await _dataImportService.UpdateStatus(dataImport.Id, DataImportStatus.STAGE_3, percentageComplete);

                // If no lines then don't create a batch unless it's the last one & there are zero
                // lines in total in which case create a zero lines batch
                if (table.Rows.Count == 0 && (batchCount != numBatches || batchFilesExist))
                {
                    _logger.LogInformation($"Skipping batch file for row count {table.Rows.Count} with batchCount {batchCount} and numBatches {numBatches} and batchFilesExist {batchFilesExist} and batch {batch.Count()}");
                    batchCount++;
                    continue;
                }

                WriteDataTableToStream(table, writer);
                await writer.FlushAsync();

                stream.Seek(0, SeekOrigin.Begin);

                var dataBlob = await _blobStorageService.GetBlob(PrivateFilesContainerName, dataImport.File.Path());

                await _blobStorageService.UploadStream(
                    containerName: PrivateFilesContainerName,
                    path: AdminReleasePath(dataImport.File.ReleaseId, FileType.Data, batchFilePath),
                    stream: stream,
                    contentType: "text/csv",
                    options: new IBlobStorageService.UploadStreamOptions
                    {
                        MetaValues = FileStorageUtils.GetDataFileMetaValues(
                            name: dataBlob.Name,
                            metaFileName: dataImport.MetaFile.Filename,
                            userName: dataBlob.GetUserName(),
                            numberOfRows: numRows
                        )
                    }
                );

                batchFilesExist = true;
                batchCount++;
            }
        }

        private static bool IsGeographicLevelIgnored(IReadOnlyList<string> line, List<string> headers)
        {
            var geographicLevel = GetGeographicLevel(line, headers);
            return IgnoredGeographicLevels.Contains(geographicLevel);
        }

        private static GeographicLevel GetGeographicLevel(IReadOnlyList<string> line, List<string> headers)
        {
            return GetGeographicLevelFromString(CsvUtil.Value(line, headers, "geographic_level"));
        }

        private static GeographicLevel GetGeographicLevelFromString(string value)
        {
            foreach (GeographicLevel val in Enum.GetValues(typeof(GeographicLevel)))
            {
                if (val.GetEnumLabel().ToLower().Equals(value.ToLower()))
                {
                    return val;
                }
            }
            throw new InvalidGeographicLevelException(value);
        }

        private static void WriteDataTableToStream(DataTable dataTable, TextWriter tw)
        {
            var csvWriter = new CsvWriter(tw, new CsvConfiguration(CultureInfo.InvariantCulture));
            foreach (DataColumn column in dataTable.Columns)
            {
                csvWriter.WriteField(column.ColumnName);
            }

            csvWriter.NextRecord();

            foreach (DataRow row in dataTable.Rows)
            {
                for (var i = 0; i < dataTable.Columns.Count; i++)
                {
                    csvWriter.WriteField(row[i]);
                }
                csvWriter.NextRecord();
            }
        }

        private static void CopyColumns(DataTable source, DataTable target)
        {
            foreach (DataColumn column in source.Columns)
            {
                column.CopyTo(target);
            }
        }

        private static void CopyRows(DataTable target, IEnumerable<DataRow> rows, List<string> headerList)
        {
            foreach (var row in from line in rows let s = CsvUtil.GetRowValues(line) where !IsGeographicLevelIgnored(s, headerList) select line)
            {
                target.Rows.Add(row.ItemArray);
            }
        }

        private static int GetBatchNumberFromBatchFileName(string batchFileName)
        {
            return Int32.Parse(batchFileName.Substring(batchFileName.LastIndexOf("_", StringComparison.Ordinal) + 1));
        }
    }
}
