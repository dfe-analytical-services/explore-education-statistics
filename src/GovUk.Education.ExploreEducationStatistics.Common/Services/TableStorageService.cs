using System.Collections.Generic;
using System.Threading.Tasks;
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
        
        public async Task<TableResult> DeleteEntityAsync(string tableName, ITableEntity entity)
        {
            var table = _client.GetTableReference(tableName);
            entity.ETag = "*";
            return await table.ExecuteAsync(TableOperation.Delete(entity));
        }
        
        public async Task<TableResult> RetrieveEntity(string tableName, ITableEntity entity, List<string> columns)
        {
            var table = _client.GetTableReference(tableName);
            return await table.ExecuteAsync(TableOperation.Retrieve(entity.PartitionKey,entity.RowKey, columns));
        }
        
        public async Task<TableResult> UpdateEntity(string tableName, ITableEntity entity)
        {
            var table = _client.GetTableReference(tableName);
            return await table.ExecuteAsync(TableOperation.InsertOrReplace(entity));
        }
    }
}