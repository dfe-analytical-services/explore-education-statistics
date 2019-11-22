using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
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
            var batches = enumerable.Skip(1).Batch(message.RowsPerBatch);
            var batchCount = 1;
            var numBatches = GetNumBatches(enumerable.Count(), message.RowsPerBatch);
            var messages = new List<ImportMessage>();

            foreach (var batch in batches)
            {
                var fileName = $"{FileStoragePathUtils.BatchesDir}/{message.DataFileName}_{batchCount:000000}";
                var lines = batch.ToList();
                var mStream = new MemoryStream();
                var writer = new StreamWriter(mStream);
                writer.Flush();

                // Insert the header at the beginning of each file/batch
                writer.WriteLine(header);

                foreach (var line in lines)
                {
                    writer.WriteLine(line);
                    writer.Flush();
                }

                await _fileStorageService.UploadDataFileAsync(
                    message.Release.Id,
                    mStream, BlobUtils.GetMetaFileName(subjectData.DataBlob),
                    BlobUtils.GetName(subjectData.DataBlob),
                    fileName,
                    "text/csv"
                );

                var iMessage = new ImportMessage
                {
                    DataFileName = fileName,
                    OrigDataFileName = message.DataFileName,
                    Release = message.Release,
                    BatchNo = batchCount,
                    NumBatches = numBatches,
                    RowsPerBatch = message.RowsPerBatch
                };

                collector.Add(iMessage);
                batchCount++;
            }
        }

        public static int GetNumBatches(int rows, int rowsPerBatch)
        {
            return (int) Math.Ceiling(rows / (double) rowsPerBatch);
        }
    }
}