#nullable enable
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Azure.Data.Tables;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;

public interface IDataTableStorageService
{
    Task<TEntity?> GetEntityIfExists<TEntity>(
        string tableName,
        string partitionKey,
        string rowKey,
        IEnumerable<string>? select = null,
        CancellationToken cancellationToken = default)
        where TEntity : class, ITableEntity;

    Task CreateEntity(
        string tableName,
        ITableEntity entity,
        CancellationToken cancellationToken = default);

    Task UpdateEntity(
        string tableName,
        ITableEntity entity,
        TableUpdateMode updateMode = TableUpdateMode.Replace,
        CancellationToken cancellationToken = default);

    Task DeleteEntity(
        string tableName,
        string partitionKey,
        string rowKey,
        CancellationToken cancellationToken = default);
}
