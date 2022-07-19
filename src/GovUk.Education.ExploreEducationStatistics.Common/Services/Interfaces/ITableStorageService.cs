#nullable enable
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces
{
    public interface ITableStorageService
    {
        Task<IEnumerable<TElement>> ExecuteQueryAsync<TElement>(string tableName, TableQuery<TElement> query)
            where TElement : ITableEntity, new();

        CloudTable GetTable(string tableName);

        Task<bool> DeleteEntityAsync(string tableName, ITableEntity entity);

        Task<TableResult> RetrieveEntity(string tableName, ITableEntity entity, List<string> columns);

        Task<TableResult> CreateOrUpdateEntity(string tableName, ITableEntity entity);
    }
}
