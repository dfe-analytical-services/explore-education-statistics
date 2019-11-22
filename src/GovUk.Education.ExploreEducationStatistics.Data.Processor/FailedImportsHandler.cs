using System;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Functions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Storage.Queue;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor
{
    public static class FailedImportsHandler
    {
        public static void CheckIncompleteImport()
        {
            var tblStorageAccount = Microsoft.Azure.Cosmos.Table.CloudStorageAccount.Parse(ConnectionUtils.GetAzureStorageConnectionString("CoreStorage"));
            var storageAccount = Microsoft.Azure.Storage.CloudStorageAccount.Parse(ConnectionUtils.GetAzureStorageConnectionString("CoreStorage"));

            var tClient = tblStorageAccount.CreateCloudTableClient();
            var qClient = storageAccount.CreateCloudQueueClient();
            var aQueue = qClient.GetQueueReference("imports-available");
            var pQueue = qClient.GetQueueReference("imports-pending");
            var table = tClient.GetTableReference("imports");

            aQueue.CreateIfNotExists();
            table.CreateIfNotExists();

            TableContinuationToken token = null;
            do
            {
                var resultSegment =
                    table.ExecuteQuerySegmentedAsync(GetQuery(), token).Result;
                token = resultSegment.ContinuationToken;

                foreach (var entity in resultSegment.Results)
                {
                    Console.Out.WriteLine(entity.Status);
                    var lastBatch = ImportStatusService.GetNumBatchesComplete(entity);
                    if (entity.NumBatches == 1 || entity.NumBatches != lastBatch)
                    {
                        // Older entries will not have this recover mechanism so have to ignore 
                        if (entity.Message != null)
                        {
                            var m = JsonConvert.DeserializeObject<ImportMessage>(entity.Message);
                            Console.Out.WriteLine($"Recovering {entity.PartitionKey} : {entity.RowKey}");

                            // If batch was not split then just processing again by adding to pending queue
                            if (entity.NumBatches == 1 || lastBatch == 0)
                            {
                                pQueue.AddMessage(new CloudQueueMessage(entity.Message));
                            }
                            else
                            // Add the batch that it failed on to the queue is missing
                            {
                                var mMissing = BuildMessage(m, lastBatch+1);
                                var messages = aQueue.GetMessages(20);
                                if (!messages.Any(x => x.AsString.Contains(mMissing)))
                                {
                                    aQueue.AddMessage(new CloudQueueMessage(mMissing));
                                }
                            }
                        }
                        else
                        {
                            Console.Out.WriteLine($"No message stored for import - unable to recover import " +
                                                  $"{entity.PartitionKey} : {entity.RowKey}");
                        }
                    }
                }
            } while (token != null);
        }
        
        private static TableQuery<DatafileImport> GetQuery()
        {
            var f1 = TableQuery.GenerateFilterCondition("Status", 
                QueryComparisons.Equal, IStatus.RUNNING_PHASE_1.GetEnumValue());
            var f2 = TableQuery.GenerateFilterCondition("Status", 
                QueryComparisons.Equal, IStatus.RUNNING_PHASE_2.GetEnumValue());
            var combineFilters = TableQuery.CombineFilters(f1, TableOperators.Or, f2);
            var f3 = TableQuery.GenerateFilterCondition("Status", 
                QueryComparisons.Equal, IStatus.RUNNING_PHASE_3.GetEnumValue());
            return new TableQuery<DatafileImport>()
                .Where(TableQuery.CombineFilters(combineFilters, TableOperators.Or, f3));
        }
        
        private static string BuildMessage(ImportMessage message, int batchNo)
        {
            var fileName = $"{FileStoragePathUtils.BatchesDir}/{message.DataFileName}_{batchNo++:000000}";

            var iMessage = new ImportMessage
            {
                DataFileName = fileName,
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