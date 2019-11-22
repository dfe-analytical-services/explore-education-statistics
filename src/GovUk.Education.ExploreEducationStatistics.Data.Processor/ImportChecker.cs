using System;
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
    public static class ImportChecker
    {
        public static void CheckIncompleteImport()
        {
            var tblStorageAccount = Microsoft.Azure.Cosmos.Table.CloudStorageAccount.Parse(ConnectionUtils.GetAzureStorageConnectionString("CoreStorage"));
            var storageAccount = Microsoft.Azure.Storage.CloudStorageAccount.Parse(ConnectionUtils.GetAzureStorageConnectionString("CoreStorage"));

            var tClient = tblStorageAccount.CreateCloudTableClient();
            var qClient = storageAccount.CreateCloudQueueClient();
            var aQueue = qClient.GetQueueReference("imports-available");
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
                    if (entity.NumBatches != lastBatch)
                    {
                        if (entity.Message != null)
                        {
                            var m = JsonConvert.DeserializeObject<ImportMessage>(entity.Message);
                            Console.Out.WriteLine(m.OrigDataFileName);
                            // Create a new message at the point the batch failed
                            //var message = BuildMessage(dataFileName, releaseId);
                            //aQueue.AddMessage(message); 
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
            var filter1 = TableQuery.GenerateFilterCondition("Status", 
                QueryComparisons.Equal, IStatus.RUNNING_PHASE_2.GetEnumValue());
            var filter2 = TableQuery.GenerateFilterCondition("Status", 
                QueryComparisons.Equal, IStatus.RUNNING_PHASE_3.GetEnumValue());
            return new TableQuery<DatafileImport>()
                .Where(TableQuery.CombineFilters(filter1, TableOperators.Or, filter2));
        }
        
//        private CloudQueueMessage BuildMessage(string dataFileName, Guid releaseId)
//        {
//            var message = new ImportMessage
//            {
//                DataFileName = dataFileName,
//                OrigDataFileName = dataFileName,
//                Release = importMessageRelease,
//                BatchNo = 1,
//                NumBatches = 1
//            };
//
//            return new CloudQueueMessage(JsonConvert.SerializeObject(message));
//        }
    }
}