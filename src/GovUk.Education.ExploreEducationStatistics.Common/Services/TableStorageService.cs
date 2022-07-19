#nullable enable
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using Microsoft.Azure.Cosmos.Table;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services
{
    public class TableStorageService : ITableStorageService
    {
        private readonly CloudTableClient _client;
        private readonly StorageInstanceCreationUtil _storageInstanceCreationUtil;

        public TableStorageService(
            string connectionString, 
            StorageInstanceCreationUtil storageInstanceCreationUtil)
        {
            var account = CloudStorageAccount.Parse(connectionString);
            _client = account.CreateCloudTableClient();
            _storageInstanceCreationUtil = storageInstanceCreationUtil;
        }

        /// <summary>
        /// Gets a table by name, will create the table if it does not exist
        /// </summary>
        /// <param name="tableName">The name of the table to get.</param>
        /// <returns>The table</returns>
        public CloudTable GetTable(string tableName)
        {
            var table = _client.GetTableReference(tableName);

            _storageInstanceCreationUtil.CreateInstanceIfNotExists(
                _client.StorageUri.ToString(),
                AzureStorageType.Table,
                tableName,
                () => table.CreateIfNotExists());

            return table;
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
            var table = GetTable(tableName);
            TableContinuationToken? token = null;
            do
            {
                var queryResult = await table.ExecuteQuerySegmentedAsync(query, token);
                results.AddRange(queryResult.Results);
                token = queryResult.ContinuationToken;
            } while (token != null);

            return results;
        }
    }
}
