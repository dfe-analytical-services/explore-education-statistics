#nullable enable
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;
using Azure.Data.Tables.Models;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services;

public class DataTableStorageService(string tableStorageConnectionString) : IDataTableStorageService
{
    private readonly TableServiceClient _client = new(tableStorageConnectionString);

    /// <summary>
    /// Gets a list of tables from the storage account
    /// </summary>
    public AsyncPageable<TableItem> GetTablesAsync(
        Expression<Func<TableItem, bool>>? filter = null,
        int? maxPerPage = null,
        CancellationToken cancellationToken = default)
    {
        filter ??= tableItem => true;

        return _client.QueryAsync(
            filter: filter,
            maxPerPage: maxPerPage,
            cancellationToken: cancellationToken);
    }

    public async Task<TEntity?> GetEntityIfExists<TEntity>(
        string tableName, 
        string partitionKey, 
        string rowKey,
        IEnumerable<string>? select = null,
        CancellationToken cancellationToken = default)
        where TEntity : class, ITableEntity
    {
        var tableClient = await GetTableClientAsync(tableName, cancellationToken);

        var createdEntity = await tableClient.GetEntityIfExistsAsync<TEntity>(
            partitionKey: partitionKey,
            rowKey: rowKey,
            select: select,
            cancellationToken: cancellationToken);

        return createdEntity.HasValue
            ? createdEntity.Value
            : null;
    }

    public async Task CreateEntity(
        string tableName, 
        ITableEntity entity,
        CancellationToken cancellationToken = default)
    {
        var tableClient = await GetTableClientAsync(tableName, cancellationToken);

        await tableClient.AddEntityAsync(
            entity: entity,
            cancellationToken: cancellationToken);
    }

    public async Task UpdateEntity(
        string tableName,
        ITableEntity entity,
        TableUpdateMode updateMode = TableUpdateMode.Replace,
        CancellationToken cancellationToken = default)
    {
        var tableClient = await GetTableClientAsync(tableName, cancellationToken);

        await tableClient.UpdateEntityAsync(
            entity: entity,
            ifMatch: entity.ETag,
            mode: updateMode,
            cancellationToken: cancellationToken);
    }

    public async Task DeleteEntity(
        string tableName, 
        string partitionKey,
        string rowKey,
        CancellationToken cancellationToken = default)
    {
        var tableClient = await GetTableClientAsync(tableName, cancellationToken);

        await tableClient.DeleteEntityAsync(
            partitionKey: partitionKey,
            rowKey: rowKey,
            ifMatch: ETag.All,
            cancellationToken: cancellationToken);
    }

    public async Task<AsyncPageable<TEntity>> QueryEntitiesAsync<TEntity>(
        string tableName,
        Expression<Func<TEntity, bool>>? filter = null,
        int? maxPerPage = 1000,
        IEnumerable<string>? select = null,
        CancellationToken cancellationToken = default)
        where TEntity : class, ITableEntity
    {
        filter ??= entity => true;

        var tableClient = await GetTableClientAsync(tableName, cancellationToken);

        return tableClient.QueryAsync(
            filter: filter,
            maxPerPage: maxPerPage,
            select: select,
            cancellationToken: cancellationToken);
    }

    public async Task BatchManipulateEntities<TEntity>(
        string tableName, 
        IEnumerable<TEntity> entities, 
        TableTransactionActionType tableTransactionActionType,
        CancellationToken cancellationToken = default)
        where TEntity : class, ITableEntity, new()
    {
        var tableClient = await GetTableClientAsync(tableName, cancellationToken);

        await DataTableStorageUtils.BatchManipulateEntities(
            tableClient: tableClient,
            entities: entities,
            tableTransactionActionType: tableTransactionActionType,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Gets a table by name, will create the table if it does not exist
    /// </summary>
    /// <param name="tableName">The name of the table to get.</param>
    /// <returns>The table</returns>
    private async Task<TableClient> GetTableClient(string tableName)
    {
        var tableClient = _client.GetTableClient(tableName);

        await tableClient.CreateIfNotExistsAsync();

        return tableClient;
    }
}