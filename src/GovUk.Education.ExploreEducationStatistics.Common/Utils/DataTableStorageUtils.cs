using Azure;
using Azure.Data.Tables;

namespace GovUk.Education.ExploreEducationStatistics.Common.Utils;

public static class DataTableStorageUtils
{
    /// <summary>
    /// Groups entities by PartitionKey into batches of max 100 for valid transactions.
    /// See <see href="https://learn.microsoft.com/en-us/rest/api/storageservices/performing-entity-group-transactions">here</see>.
    /// </summary>
    /// <returns>List of Azure Responses for Transactions</returns>
    internal static async Task<IReadOnlyList<Response<IReadOnlyList<Response>>>> BatchManipulateEntities<TEntity>(
        TableClient tableClient,
        IEnumerable<TEntity> entities,
        TableTransactionActionType tableTransactionActionType,
        CancellationToken cancellationToken = default
    )
        where TEntity : class, ITableEntity
    {
        var groups = entities.GroupBy(entity => entity.PartitionKey);

        var responses = new List<Response<IReadOnlyList<Response>>>();

        foreach (var group in groups)
        {
            var items = group.AsEnumerable();

            while (items.Any())
            {
                var batch = items.Take(100);
                items = items.Skip(100);

                var actions = batch.Select(entity => new TableTransactionAction(tableTransactionActionType, entity));

                var response = await tableClient.SubmitTransactionAsync(actions, cancellationToken);

                responses.Add(response);
            }
        }
        return responses;
    }
}
