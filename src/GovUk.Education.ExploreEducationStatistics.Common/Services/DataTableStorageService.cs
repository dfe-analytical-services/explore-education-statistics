#nullable enable
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services;

public class DataTableStorageService(string tableStorageConnectionString) : IDataTableStorageService
{
    private readonly TableServiceClient _client = new(tableStorageConnectionString);

    public async Task<TEntity?> GetEntityIfExists<TEntity>(
        string tableName, 
        string partitionKey, 
        string rowKey,
        IEnumerable<string>? select = null,
        CancellationToken cancellationToken = default)
        where TEntity : class, ITableEntity
    {
        var tableClient = await GetTableClient(tableName);

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
        var tableClient = await GetTableClient(tableName);

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
        var tableClient = await GetTableClient(tableName);

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
        var tableClient = await GetTableClient(tableName);

        await tableClient.DeleteEntityAsync(
            partitionKey: partitionKey,
            rowKey: rowKey,
            ifMatch: ETag.All,
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
