using System;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Storage.Queue;
using Newtonsoft.Json;
using static GovUk.Education.ExploreEducationStatistics.Common.TableStorageTableNames;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor
{
    public static class FailedImportsHandler
    {
        public static void CheckIncompleteImports(
            string tableStorageConnectionString,
            IFileStorageService fileStorageService)
        {
            var tblStorageAccount = CloudStorageAccount.Parse(tableStorageConnectionString);
            var storageAccount = Microsoft.Azure.Storage.CloudStorageAccount.Parse(tableStorageConnectionString);

            var tableClient = tblStorageAccount.CreateCloudTableClient();
            var queueClient = storageAccount.CreateCloudQueueClient();
            var availableQueue = queueClient.GetQueueReference("imports-available");
            var pendingQueue = queueClient.GetQueueReference("imports-pending");
            var table = tableClient.GetTableReference(DatafileImportsTableName);

            availableQueue.CreateIfNotExists();
            pendingQueue.CreateIfNotExists();
            table.CreateIfNotExists();
            availableQueue.Clear();
            pendingQueue.Clear();

            TableContinuationToken token = null;
            do
            {
                var resultSegment =
                    table.ExecuteQuerySegmentedAsync(BuildQuery(), token).Result;
                token = resultSegment.ContinuationToken;

                foreach (var entity in resultSegment.Results)
                {
                    Console.Out.WriteLine($"Recovering {entity.PartitionKey} : {entity.RowKey}");

                    // If batch was not split then just processing again by adding to pending queue
                    if (entity.Status.Equals(IStatus.QUEUED) || entity.Status.Equals(IStatus.PROCESSING_ARCHIVE_FILE) || entity.Status.Equals(IStatus.RUNNING_PHASE_1))
                    {
                        Console.WriteLine($"No data imported for {entity.PartitionKey} : {entity.RowKey} - Import will be re-run");
                        pendingQueue.AddMessage(new CloudQueueMessage(entity.Message));
                    }
                    // Else create a message for all remaining batches
                    else
                    {
                        var m = JsonConvert.DeserializeObject<ImportMessage>(entity.Message);
                        var batches = fileStorageService.GetBatchesRemaining(entity.PartitionKey, m.OrigDataFileName)
                            .Result
                            .ToList();

                        // If no batches then assume it didn't get passed initial validation stage
                        if (!batches.Any())
                        {
                            pendingQueue.AddMessage(new CloudQueueMessage(entity.Message));
                            return;
                        }

                        foreach (var folderAndFilename in batches)
                        {
                            availableQueue.AddMessage(new CloudQueueMessage(BuildMessage(m, folderAndFilename.Path)));
                        }
                    }
                }
            } while (token != null);
        }

        private static TableQuery<DatafileImport> BuildQuery()
        {
            var combineFilters = TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition("Status",
                    QueryComparisons.Equal, IStatus.QUEUED.ToString())
                , TableOperators.Or,
                TableQuery.GenerateFilterCondition("Status",
                    QueryComparisons.Equal, IStatus.RUNNING_PHASE_1.ToString()));

            combineFilters = TableQuery.CombineFilters(
                combineFilters,
                TableOperators.Or,
                TableQuery.GenerateFilterCondition("Status",
                    QueryComparisons.Equal, IStatus.RUNNING_PHASE_2.ToString()));

            combineFilters = TableQuery.CombineFilters(
                combineFilters,
                TableOperators.Or,
                TableQuery.GenerateFilterCondition("Status",
                    QueryComparisons.Equal, IStatus.PROCESSING_ARCHIVE_FILE.ToString()));

            return new TableQuery<DatafileImport>()
                .Where(TableQuery.CombineFilters(
                    combineFilters,
                    TableOperators.Or,
                    TableQuery.GenerateFilterCondition("Status",
                    QueryComparisons.Equal, IStatus.RUNNING_PHASE_3.ToString())));
        }

        private static string BuildMessage(ImportMessage message, string folderAndFilename)
        {
            var fileName = folderAndFilename.Split(FileStoragePathUtils.BatchesDir + "/")[1];
            var batchNo = Int16.Parse(fileName.Substring(fileName.Length-6));

            Console.WriteLine($"Recreating message queue for {fileName}");

            var iMessage = new ImportMessage
            {
                SubjectId = message.SubjectId,
                DataFileName = $"{FileStoragePathUtils.BatchesDir}/{fileName}",
                OrigDataFileName = message.DataFileName,
                Release = message.Release,
                BatchNo = batchNo,
                NumBatches = message.NumBatches,
                RowsPerBatch = message.RowsPerBatch,
                TotalRows = message.TotalRows
            };

            return JsonConvert.SerializeObject(iMessage);
        }
    }
}