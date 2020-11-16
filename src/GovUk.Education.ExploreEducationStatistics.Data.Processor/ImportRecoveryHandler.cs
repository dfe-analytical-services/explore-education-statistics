using System;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Storage.Queue;
using static GovUk.Education.ExploreEducationStatistics.Common.TableStorageTableNames;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor
{
    public static class ImportRecoveryHandler
    {
        public static void CheckIncompleteImports(string connectionString)
        {
            var tblStorageAccount = CloudStorageAccount.Parse(connectionString);
            var storageAccount = Microsoft.Azure.Storage.CloudStorageAccount.Parse(connectionString);

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
                    Console.Out.WriteLine($"Recovering interrupted import for {entity.PartitionKey} : {entity.RowKey}");
                    pendingQueue.AddMessage(new CloudQueueMessage(entity.Message));
                }
            } while (token != null);
        }

        private static TableQuery<DatafileImport> BuildQuery()
        {
            var combineFilters = TableQuery.GenerateFilterCondition("Status",
                    QueryComparisons.Equal, IStatus.QUEUED.ToString());

            IStatus[] statuses = {
                IStatus.PROCESSING_ARCHIVE_FILE,
                IStatus.STAGE_1,
                IStatus.STAGE_2,
                IStatus.STAGE_3,
                IStatus.STAGE_4
            };

            combineFilters = statuses.Aggregate(combineFilters, (current, status) =>
                TableQuery.CombineFilters(current,
                    TableOperators.Or,
                    TableQuery.GenerateFilterCondition("Status", QueryComparisons.Equal, status.ToString())));

            return new TableQuery<DatafileImport>().Where(combineFilters);
        }
    }
}