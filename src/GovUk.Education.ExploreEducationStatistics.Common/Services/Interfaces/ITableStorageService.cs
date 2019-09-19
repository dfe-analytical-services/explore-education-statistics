using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces
{
    public interface ITableStorageService
    {
        Task<CloudTable> GetTableAsync(string tableName, bool createIfNotExists = true);
        Task<TableResult> DeleteEntityAsync(string tableName, ITableEntity entity);
        Task<TableResult> RetrieveEntity(string tableName, ITableEntity entity, List<string> columns);
        Task<TableResult> UpdateEntity(string tableName, ITableEntity entity);
    }
}