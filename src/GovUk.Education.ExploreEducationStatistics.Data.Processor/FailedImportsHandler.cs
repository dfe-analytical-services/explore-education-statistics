using System;
using GovUk.Education.ExploreEducationStatistics.Common.Functions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Storage.Queue;
using Newtonsoft.Json;
using Microsoft.Azure.Storage.Blob;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor
{
    public static class FailedImportsHandler
    {
        public static void CheckIncompleteImports()
        {
            var tblStorageAccount = CloudStorageAccount.Parse(ConnectionUtils.GetAzureStorageConnectionString("CoreStorage"));
            var storageAccount = Microsoft.Azure.Storage.CloudStorageAccount.Parse(ConnectionUtils.GetAzureStorageConnectionString("CoreStorage"));
            CloudBlobContainer container = FileStorageService.GetOrCreateBlobContainer(ConnectionUtils.GetAzureStorageConnectionString("CoreStorage")).Result;
            var tClient = tblStorageAccount.CreateCloudTableClient();
            var qClient = storageAccount.CreateCloudQueueClient();
            var aQueue = qClient.GetQueueReference("imports-available");
            var pQueue = qClient.GetQueueReference("imports-pending");
            var table = tClient.GetTableReference("imports");

            aQueue.CreateIfNotExists();
            table.CreateIfNotExists();
            //aQueue.Clear();
            //pQueue.Clear();

            TableContinuationToken token = null;
            do
            {
                var resultSegment =
                    table.ExecuteQuerySegmentedAsync(BuildQuery(), token).Result;
                token = resultSegment.ContinuationToken;

                foreach (var entity in resultSegment.Results)
                {
                    var lastBatch = FileStorageService.GetNumBatchesComplete(entity.PartitionKey, container, entity.NumBatches);
                    if (entity.NumBatches != 1 && entity.NumBatches == lastBatch)
                    {
                        continue;
                    }

                    // Older entries will not have this recover mechanism so have to ignore 
                    if (entity.Message != null)
                    {
                        Console.Out.WriteLine($"Recovering {entity.PartitionKey} : {entity.RowKey}");

                        // If batch was not split then just processing again by adding to pending queue
                        if (entity.NumBatches == 1 || lastBatch == 0)
                        {
                            pQueue.AddMessage(new CloudQueueMessage(entity.Message));
                        }
                        else
                            // Add the batch that failed on to the queue is missing
                        {
                            aQueue.AddMessage(new CloudQueueMessage(BuildMessage(
                                JsonConvert.DeserializeObject<ImportMessage>(entity.Message), 
                                lastBatch+1)));
                        }
                    }
                    else
                    {
                        Console.Out.WriteLine($"No message stored for import - unable to recover import " +
                                              $"{entity.PartitionKey} : {entity.RowKey}");
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
        
        private static string BuildMessage(ImportMessage message, int batchNo)
        {
            var iMessage = new ImportMessage
            {
                DataFileName = $"{FileStoragePathUtils.BatchesDir}/{message.DataFileName}_{batchNo:000000}",
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