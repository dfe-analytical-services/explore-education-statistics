using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services
{
    public class SplitFileService : ISplitFileService
    {
        private readonly IFileStorageService _fileStorageService;
        
        private static readonly List<GeographicLevel> IgnoredGeographicLevels = new List<GeographicLevel>
        {
            GeographicLevel.Institution,
            GeographicLevel.Provider,
            GeographicLevel.School
        };
        
        public SplitFileService(IFileStorageService fileStorageService)
        {
            _fileStorageService = fileStorageService;
        }

        public async Task SplitDataFile(
            ICollector<ImportMessage> collector,
            ImportMessage message,
            SubjectData subjectData)
        {
            var csvLines = subjectData.GetCsvLines().ToList();

            if (csvLines.Count() > message.RowsPerBatch + 1)
            {
                await SplitFiles(message, csvLines, subjectData, collector);
            }
            // Else perform any additional validation & pass on file to message queue for import
            else
            {
                collector.Add(message);
            }
        }

        private async Task SplitFiles(
            ImportMessage message,
            IEnumerable<string> csvLines,
            SubjectData subjectData,
            ICollector<ImportMessage> collector)
        {
            var enumerable = csvLines.ToList();
            var header = enumerable.First();
            var headers = header.Split(',').ToList();
            var batches = enumerable.Skip(1).Batch(message.RowsPerBatch);
            var batchCount = 1;
            var numRows = enumerable.Count();
            var numBatches = GetNumBatches(numRows, message.RowsPerBatch);
            var messages = new List<ImportMessage>();

            foreach (var batch in batches)
            {
                var fileName = $"{FileStoragePathUtils.BatchesDir}/{message.DataFileName}_{batchCount:000000}";
                var lines = batch.ToList();
                var mStream = new MemoryStream();
                var writer = new StreamWriter(mStream);
                var actualLines = 0;
                writer.Flush();

                // Insert the header at the beginning of each file/batch
                writer.WriteLine(header);
                foreach (var line in from line in lines let s = line.Split(',') where !IsGeographicLevelIgnored(s, headers) select line)
                {
                    writer.WriteLine(line);
                    writer.Flush();
                    actualLines++;
                }
                
                // If no lines then don't create a batch or message
                if (actualLines == 0)
                {
                    continue;
                }

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
                    NumBatches = numBatches,
                    RowsPerBatch = message.RowsPerBatch
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

        public static int GetNumBatches(int rows, int rowsPerBatch)
        {
            return (int) Math.Ceiling(rows / (double) rowsPerBatch);
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
    }
}