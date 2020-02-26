using System;
using GovUk.Education.ExploreEducationStatistics.Common.Functions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Storage.Queue;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor
{
    public static class FailedImportsHandler
    {
        public static void CheckIncompleteImports()
        {
            var tblStorageAccount = CloudStorageAccount.Parse(ConnectionUtils.GetAzureStorageConnectionString("CoreStorage"));
            var storageAccount = Microsoft.Azure.Storage.CloudStorageAccount.Parse(ConnectionUtils.GetAzureStorageConnectionString("CoreStorage"));
            var container = FileStorageService.GetOrCreateBlobContainer(ConnectionUtils.GetAzureStorageConnectionString("CoreStorage")).Result;
            var tClient = tblStorageAccount.CreateCloudTableClient();
            var qClient = storageAccount.CreateCloudQueueClient();
            var aQueue = qClient.GetQueueReference("imports-available");
            var pQueue = qClient.GetQueueReference("imports-pending");
            var table = tClient.GetTableReference("imports");

            aQueue.CreateIfNotExists();
            table.CreateIfNotExists();
            aQueue.Clear();
            //pQueue.Clear();

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
                    if (entity.NumBatches == 1)
                    {
                        pQueue.AddMessage(new CloudQueueMessage(entity.Message));
                    }
                    // Else create a message for all remaining batches
                    else
                    {
                        ImportMessage m = JsonConvert.DeserializeObject<ImportMessage>(entity.Message);

                        foreach (var folderAndFilename in FileStorageService.GetBatchesRemaining(entity.PartitionKey, container, m.OrigDataFileName))
                        {
                            aQueue.AddMessage(new CloudQueueMessage(BuildMessage(m, folderAndFilename)));
                        }
                    }
                }
            } while (token != null);
        }
        
        private static TableQuery<DatafileImport> BuildQuery()
        {
            var f1 = TableQuery.GenerateFilterCondition("Status", 
                QueryComparisons.Equal, IStatus.RUNNING_PHASE_1.ToString());
            var f2 = TableQuery.GenerateFilterCondition("Status", 
                QueryComparisons.Equal, IStatus.RUNNING_PHASE_2.ToString());
            var combineFilters = TableQuery.CombineFilters(f1, TableOperators.Or, f2);
            var f3 = TableQuery.GenerateFilterCondition("Status", 
                QueryComparisons.Equal, IStatus.RUNNING_PHASE_3.ToString());
            return new TableQuery<DatafileImport>()
                .Where(TableQuery.CombineFilters(combineFilters, TableOperators.Or, f3));
        }
        
        private static string BuildMessage(ImportMessage message, string folderAndFilename)
        {
            var fileName = folderAndFilename.Split(FileStoragePathUtils.BatchesDir + "/")[1];
            var batchNo = Int16.Parse(fileName.Substring(fileName.Length-6));
            
            var iMessage = new ImportMessage
            {
                DataFileName = $"{FileStoragePathUtils.BatchesDir}/{fileName}",
                OrigDataFileName = message.DataFileName,
                Release = message.Release,
                BatchNo = batchNo,
                NumBatches = message.NumBatches,
                RowsPerBatch = message.RowsPerBatch
            };

            return JsonConvert.SerializeObject(iMessage);
        }
    }
}