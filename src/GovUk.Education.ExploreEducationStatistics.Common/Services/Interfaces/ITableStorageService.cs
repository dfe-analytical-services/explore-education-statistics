using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces
{
    public interface ITableStorageService
    {
        Task<IEnumerable<TElement>> ExecuteQueryAsync<TElement>(string tableName, TableQuery<TElement> query) where TElement : ITableEntity, new();
        Task<CloudTable> GetTableAsync(string tableName, bool createIfNotExists = true);
        Task DeleteByPartitionKey(string tableName, string partitionKey);
        Task DeleteByPartitionKeys(string tableName, IEnumerable<string> partitionKeys);
        Task<bool> DeleteEntityAsync(string tableName, ITableEntity entity);
        Task<TableResult> RetrieveEntity(string tableName, ITableEntity entity, List<string> columns);
        Task<TableResult> CreateOrUpdateEntity(string tableName, ITableEntity entity);
    }
}