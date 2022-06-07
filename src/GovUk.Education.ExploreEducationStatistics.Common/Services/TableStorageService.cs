using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
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

        public async Task DeleteByPartitionKeys(string tableName, IEnumerable<string> partitionKeys)
        {
            var block = new ActionBlock<(CloudTable table, string partitionKey)>(
                async tuple => { await DeleteByPartitionKey(tuple.table, tuple.partitionKey); },
                new ExecutionDataflowBlockOptions
                {
                    BoundedCapacity = 100,
                    MaxDegreeOfParallelism = 16
                });

            var table = await GetTableAsync(tableName);
            foreach (var partitionKey in partitionKeys)
            {
                await block.SendAsync((table, partitionKey));
            }

            block.Complete();
            await block.Completion;
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

                var batches = queryResult.Batch(100);
                foreach (var batch in batches)
                {
                    var tableBatchOperation = new TableBatchOperation();
                    foreach (var entity in batch)
                    {
                        tableBatchOperation.Add(TableOperation.Delete(entity));
                    }

                    await table.ExecuteBatchAsync(tableBatchOperation);
                }
            } while (token != null);
        }
    }
}