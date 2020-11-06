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
            ICollector<ImportMessage> collector,
            ImportMessage message,
            SubjectData subjectData)
        {
            await _importStatusService.UpdateStatus(message.Release.Id, message.OrigDataFileName, IStatus.STAGE_3);
            await using var dataFileStream = await _fileStorageService.StreamBlob(subjectData.DataBlob);
            
            var dataFileTable = DataTableUtils.CreateFromStream(dataFileStream);

            if (dataFileTable.Rows.Count > message.RowsPerBatch)
            {
                _logger.LogInformation($"Splitting Datafile: {message.DataFileName}");
                await SplitFiles(message, subjectData, dataFileTable, collector);
                _logger.LogInformation($"Split of Datafile: {message.DataFileName} complete");
            }
            // Else perform any additional validation & pass on file to message queue for import
            else
            {
                collector.Add(message);
            }
        }

        private async Task SplitFiles(
            ImportMessage message,
            SubjectData subjectData,
            DataTable dataFileTable,
            ICollector<ImportMessage> collector)
        {
            var headerList = CsvUtil.GetColumnValues(dataFileTable.Columns);
            var batches = dataFileTable.Rows.OfType<DataRow>().Batch(message.RowsPerBatch);
            var batchCount = 1;
            var numRows = dataFileTable.Rows.Count + 1;
            var messages = new List<ImportMessage>();
            var numBatches = (int)Math.Ceiling((double)dataFileTable.Rows.Count / message.RowsPerBatch);

            foreach (var batch in batches)
            {
                var fileName = $"{FileStoragePathUtils.BatchesDir}/{message.DataFileName}_{batchCount:000000}";
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
                if (table.Rows.Count == 0 && batchCount != numBatches || table.Rows.Count == 0 && messages.Count != 0)
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
                    fileName: fileName,
                    stream: stream,
                    contentType: "text/csv",
                    FileStorageUtils.GetDataFileMetaValues(
                        name: subjectData.DataBlob.Name,
                        metaFileName: subjectData.DataBlob.GetMetaFileName(),
                        userName: subjectData.DataBlob.GetUserName(),
                        numberOfRows: numRows
                    )
                );

                var iMessage = new ImportMessage
                {
                    SubjectId = message.SubjectId,
                    DataFileName = fileName,
                    OrigDataFileName = message.DataFileName,
                    Release = message.Release,
                    BatchNo = batchCount,
                    NumBatches = message.NumBatches,
                    RowsPerBatch = message.RowsPerBatch,
                    TotalRows = message.TotalRows
                };

                messages.Add(iMessage);

                batchCount++;
            }

            // Ensure generated messages are added after batch creation.
            foreach (var m in messages)
            {
                collector.Add(m);
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
    }
}