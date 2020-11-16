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
using GovUk.Education.ExploreEducationStatistics.Data.Importer.Exceptions;
using GovUk.Education.ExploreEducationStatistics.Data.Importer.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Utils;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services
{
    public class SplitFileService : ISplitFileService
    {
        private readonly IFileStorageService _fileStorageService;
        private readonly ILogger<ISplitFileService> _logger;
        private readonly IImportStatusService _importStatusService;

        private static readonly List<GeographicLevel> IgnoredGeographicLevels = new List<GeographicLevel>
        {
            GeographicLevel.Institution,
            GeographicLevel.Provider,
            GeographicLevel.School,
            GeographicLevel.PlanningArea
        };

        public SplitFileService(
            IFileStorageService fileStorageService,
            ILogger<ISplitFileService> logger,
            IImportStatusService importStatusService
            )
        {
            _fileStorageService = fileStorageService;
            _logger = logger;
            _importStatusService = importStatusService;
        }

        public async Task SplitDataFile(
            ImportMessage message,
            SubjectData subjectData)
        {
            await _importStatusService.UpdateStatus(message.Release.Id, message.OrigDataFileName, IStatus.STAGE_3);
            await using var dataFileStream = await _fileStorageService.StreamBlob(subjectData.DataBlob);
            
            var dataFileTable = DataTableUtils.CreateFromStream(dataFileStream);

            if (dataFileTable.Rows.Count > message.RowsPerBatch)
            {
                _logger.LogInformation($"Splitting Datafile: {message.DataFileName}");
                await SplitFiles(message, subjectData, dataFileTable);
                _logger.LogInformation($"Split of Datafile: {message.DataFileName} complete");
            }
            else
            {
                _logger.LogInformation($"No splitting of datafile: {message.DataFileName} was necessary");
            }
        }

        public async Task CreateDataFileProcessingMessages(
            ICollector<ImportMessage> collector, 
            ImportMessage message)
        {
            var batchFilesForDataFile = await _fileStorageService.GetBatchFilesForDataFile(
                message.Release.Id.ToString(), 
                message.DataFileName);

            // If no batching was necessary, simply add a message to process the lone data file
            if (!batchFilesForDataFile.Any())
            {
                collector.Add(message);
                return;
            }
            
            // Otherwise create a message per batch file to process
            var importBatchFileMessages = batchFilesForDataFile.Select(blobInfo =>
            {
                var batchFileName = blobInfo.FileName;
                var batchFilePath = $"{FileStoragePathUtils.BatchesDir}/{batchFileName}";
                var batchNo = GetBatchNumberFromBatchFileName(batchFileName);
                
                return new ImportMessage
                {
                    SubjectId = message.SubjectId,
                    DataFileName = batchFilePath,
                    OrigDataFileName = message.DataFileName,
                    Release = message.Release,
                    BatchNo = batchNo,
                    NumBatches = message.NumBatches,
                    RowsPerBatch = message.RowsPerBatch,
                    TotalRows = message.TotalRows
                };
            });
            
            foreach (var importMessage in importBatchFileMessages)
            {
                collector.Add(importMessage);
            }
        }

        private async Task SplitFiles(
            ImportMessage message,
            SubjectData subjectData,
            DataTable dataFileTable)
        {
            var headerList = CsvUtil.GetColumnValues(dataFileTable.Columns);
            var batches = dataFileTable.Rows.OfType<DataRow>().Batch(message.RowsPerBatch);
            var batchCount = 1;
            var numRows = dataFileTable.Rows.Count + 1;
            var numBatches = (int)Math.Ceiling((double)dataFileTable.Rows.Count / message.RowsPerBatch);

            var existingBatchFiles = await _fileStorageService.GetBatchFilesForDataFile(
                message.Release.Id.ToString(), 
                message.DataFileName);

            var existingBatchFileNumbers = existingBatchFiles
                .AsQueryable()
                .Select(blobInfo => GetBatchNumberFromBatchFileName(blobInfo.FileName));

            var batchFilesExist = existingBatchFileNumbers.Any();
            
            foreach (var batch in batches)
            {
                var batchFileName = $"{message.DataFileName}_{batchCount:000000}";

                if (existingBatchFileNumbers.Contains(batchCount))
                {
                    _logger.LogInformation($"Batch file {batchFileName} already exists - not recreating");
                    batchCount++;
                    continue;    
                }
                
                var batchFilePath = $"{FileStoragePathUtils.BatchesDir}/{batchFileName}";

                await using var stream = new MemoryStream();
                var writer = new StreamWriter(stream);
                await writer.FlushAsync();

                var table = new DataTable();
                CopyColumns(dataFileTable, table);
                CopyRows(table, batch.ToList(), headerList);

                var percentageComplete = (double) batchCount / numBatches * 100;

                await _importStatusService.UpdateStatus(message.Release.Id,
                    message.OrigDataFileName,
                    IStatus.STAGE_3,
                    percentageComplete);

                // If no lines then don't create a batch or message unless it's the last one & there are zero
                // lines in total in which case create a zero lines batch
                if (table.Rows.Count == 0 && (batchCount != numBatches || batchFilesExist))
                {
                    batchCount++;
                    continue;
                }

                WriteDataTableToStream(table, writer);
                await writer.FlushAsync();

                stream.Seek(0, SeekOrigin.Begin);

                await _fileStorageService.UploadStream(
                    message.Release.Id,
                    fileType: ReleaseFileTypes.Data,
                    fileName: batchFilePath,
                    stream: stream,
                    contentType: "text/csv",
                    FileStorageUtils.GetDataFileMetaValues(
                        name: subjectData.DataBlob.Name,
                        metaFileName: subjectData.DataBlob.GetMetaFileName(),
                        userName: subjectData.DataBlob.GetUserName(),
                        numberOfRows: numRows
                    )
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