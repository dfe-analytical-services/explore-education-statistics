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
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Importer.Exceptions;
using GovUk.Education.ExploreEducationStatistics.Data.Importer.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Extensions;
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
        
        private static readonly List<GeographicLevel> IgnoredGeographicLevels = new List<GeographicLevel>
        {
            GeographicLevel.Institution,
            GeographicLevel.Provider,
            GeographicLevel.School
        };
        
        public SplitFileService(IFileStorageService fileStorageService, ILogger<ISplitFileService> logger)
        {
            _fileStorageService = fileStorageService;
            _logger = logger;
        }

        public async Task SplitDataFile(
            ICollector<ImportMessage> collector,
            ImportMessage message,
            SubjectData subjectData)
        {
            if (subjectData.GetCsvTable().Rows.Count > message.RowsPerBatch)
            {
                _logger.LogInformation($"Splitting Datafile: {message.DataFileName}");
                await SplitFiles(message, subjectData, collector);
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
            ICollector<ImportMessage> collector)
        {
            var csvTable = subjectData.GetCsvTable();
            var headerList = CsvUtil.GetColumnValues(csvTable.Columns);
            var batches = csvTable.Rows.OfType<DataRow>().Batch(message.RowsPerBatch);
            var batchCount = 1;
            var numRows = csvTable.Rows.Count + 1;
            var messages = new List<ImportMessage>();

            foreach (var batch in batches)
            {
                var fileName = $"{FileStoragePathUtils.BatchesDir}/{message.DataFileName}_{batchCount:000000}";
                var mStream = new MemoryStream();
                var writer = new StreamWriter(mStream);
                writer.Flush();
                
                var table = new DataTable();
                CopyColumns(csvTable, table);
                CopyRows(table, batch.ToList(), headerList);

                // If no lines then don't create a batch or message
                if (table.Rows.Count == 0)
                {
                    continue;
                }

                WriteDataTableToStream(table, writer);
                writer.Flush();

                await _fileStorageService.UploadDataFileAsync(
                    message.Release.Id,
                    mStream, 
                    BlobUtils.GetMetaFileName(subjectData.DataBlob),
                    BlobUtils.GetName(subjectData.DataBlob),
                    fileName,
                    "text/csv",
                    numRows
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