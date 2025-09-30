#nullable enable
using System.Linq.Expressions;
using Azure;
using Azure.Data.Tables;
using Azure.Data.Tables.Models;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;

public interface IDataTableStorageService
{
    AsyncPageable<TableItem> GetTables(
        Expression<Func<TableItem, bool>>? filter = null,
        int? maxPerPage = null,
        CancellationToken cancellationToken = default
    );

    Task<TEntity?> GetEntityIfExists<TEntity>(
        string tableName,
        string partitionKey,
        string rowKey,
        IEnumerable<string>? select = null,
        CancellationToken cancellationToken = default
    )
        where TEntity : class, ITableEntity;

    Task CreateEntity(
        string tableName,
        ITableEntity entity,
        CancellationToken cancellationToken = default
    );

    Task UpdateEntity(
        string tableName,
        ITableEntity entity,
        TableUpdateMode updateMode = TableUpdateMode.Replace,
        CancellationToken cancellationToken = default
    );

    Task DeleteEntity(
        string tableName,
        string partitionKey,
        string rowKey,
        CancellationToken cancellationToken = default
    );

    Task<AsyncPageable<TEntity>> QueryEntities<TEntity>(
        string tableName,
        Expression<Func<TEntity, bool>>? filter = null,
        int? maxPerPage = 1000,
        IEnumerable<string>? select = null,
        CancellationToken cancellationToken = default
    )
        where TEntity : class, ITableEntity;

    Task<AsyncPageable<TEntity>> QueryEntities<TEntity>(
        string tableName,
        string filterStr = "",
        int? maxPerPage = 1000,
        IEnumerable<string>? select = null,
        CancellationToken cancellationToken = default
    )
        where TEntity : class, ITableEntity;

    Task BatchManipulateEntities<TEntity>(
        string tableName,
        IEnumerable<TEntity> entities,
        TableTransactionActionType tableTransactionActionType,
        CancellationToken cancellationToken = default
    )
        where TEntity : class, ITableEntity;
}
