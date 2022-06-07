using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using Microsoft.Azure.Cosmos.Table;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services
{
    public class TableStorageService : ITableStorageService
    {
        private readonly CloudTableClient _client;

        public TableStorageService(string connectionString)
        {
            var account = CloudStorageAccount.Parse(connectionString);
            _client = account.CreateCloudTableClient();
        }

        /// <summary>
        /// Gets a table by name, will create the table if it does not exist
        /// </summary>
        /// <param name="tableName">The name of the table to get.</param>
        /// <param name="createIfNotExists">Creates the table if it does not already exist, defaults to true.</param>
        /// <returns>The table</returns>
        public async Task<CloudTable> GetTableAsync(string tableName, bool createIfNotExists = true)
        {
            var table = _client.GetTableReference(tableName);

            if (createIfNotExists)
            {
                await table.CreateIfNotExistsAsync();
            }

            return table;
        }

        // public async Task DeleteByPartitionKeys(string tableName, IEnumerable<string> partitionKeys)
        // {
        //     var block = new ActionBlock<(CloudTable table, string partitionKey)>(
        //         async tuple => { await DeleteByPartitionKey(tuple.table, tuple.partitionKey); },
        //         new ExecutionDataflowBlockOptions
        //         {
        //             BoundedCapacity = 100,
        //             MaxDegreeOfParallelism = 16
        //         });
        //
        //     var table = await GetTableAsync(tableName);
        //     foreach (var partitionKey in partitionKeys)
        //     {
        //         await block.SendAsync((table, partitionKey));
        //     }
        //
        //     block.Complete();
        //     await block.Completion;
        // }
        
        public async Task DeleteByPartitionKeys(string tableName, IEnumerable<string> partitionKeys)
        {
            var table = await GetTableAsync(tableName);

            foreach (var partitionKey in partitionKeys)
            {
                TableQuery<TableEntity> deleteQuery = new TableQuery<TableEntity>()
                    .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey)
                            // TableOperators.And,
                            // TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.LessThan, timestamp)
                        // )
                    )
                    .Select(new string[] { "PartitionKey", "RowKey" });

                TableContinuationToken continuationToken = null;

                do
                {
                    var tableQueryResult = await table.ExecuteQuerySegmentedAsync(deleteQuery, continuationToken);

                    continuationToken = tableQueryResult.ContinuationToken;

                    foreach (var row in tableQueryResult)
                    {
                        await table.ExecuteAsync(TableOperation.Delete(row));
                    }
                    // // Split into chunks of 100 for batching
                    // List<List<TableEntity>> rowsChunked = tableQueryResult.Result.Select((x, index) => new { Index = index, Value = x })
                    //     .Where(x => x.Value != null)
                    //     .GroupBy(x => x.Index / 100)
                    //     .Select(x => x.Select(v => v.Value).ToList())
                    //     .ToList();
                    //
                    // // Delete each chunk of 100 in a batch
                    // foreach (List<TableEntity> rows in rowsChunked)
                    // {
                    //     TableBatchOperation tableBatchOperation = new TableBatchOperation();
                    //     rows.ForEach(x => tableBatchOperation.Add(TableOperation.Delete(x)));
                    //
                    //     await table.ExecuteBatchAsync(tableBatchOperation);
                    // }
                }
                while (continuationToken != null);
            }
        }

        public async Task DeleteByPartitionKey(string tableName, string partitionKey)
        {
            var table = await GetTableAsync(tableName);
            await DeleteByPartitionKey(table, partitionKey);
        }

        public async Task<bool> DeleteEntityAsync(string tableName, ITableEntity entity)
        {
            try
            {
                var table = _client.GetTableReference(tableName);
                entity.ETag = "*";
                var result = await table.ExecuteAsync(TableOperation.Delete(entity));
                return result != null;
            }
            catch
            {
                return false;
            }
        }

        public async Task<TableResult> RetrieveEntity(string tableName, ITableEntity entity, List<string> columns)
        {
            var table = _client.GetTableReference(tableName);
            return await table.ExecuteAsync(TableOperation.Retrieve(entity.PartitionKey, entity.RowKey, columns));
        }

        public async Task<TableResult> CreateOrUpdateEntity(string tableName, ITableEntity entity)
        {
            var table = _client.GetTableReference(tableName);
            return await table.ExecuteAsync(TableOperation.InsertOrReplace(entity));
        }

        public async Task<IEnumerable<TElement>> ExecuteQueryAsync<TElement>(string tableName,
            TableQuery<TElement> query) where TElement : ITableEntity, new()
        {
            var results = new List<TElement>();
            var table = await GetTableAsync(tableName);
            TableContinuationToken token = null;
            do
            {
                var queryResult = await table.ExecuteQuerySegmentedAsync(query, token);
                results.AddRange(queryResult.Results);
                token = queryResult.ContinuationToken;
            } while (token != null);

            return results;
        }

        private static async Task DeleteByPartitionKey(CloudTable table, string partitionKey)
        {
            var filter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey);
            var query = new TableQuery<TableEntity>().Where(filter);

            TableContinuationToken token = null;
            do
            {
                var queryResult = await table.ExecuteQuerySegmentedAsync(query, token);
                token = queryResult.ContinuationToken;

                // var batches = queryResult.Batch(100);
                // foreach (var batch in batches)
                // {
                //     var tableBatchOperation = new TableBatchOperation();
                //     foreach (var entity in batch)
                //     {
                //         tableBatchOperation.Add(TableOperation.Delete(entity));
                //     }
                //
                //     await table.ExecuteBatchAsync(tableBatchOperation);
                // }
                foreach (var row in queryResult)
                {
                    await table.ExecuteAsync(TableOperation.Delete(row));
                }
            } while (token != null);
        }
    }
}