using System;
using Azure.Data.Tables;
using Azure;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;

namespace GovUk.Education.ExploreEducationStatistics.Common.Utils;

public static class DataTableStorageUtils
{
    public static string CombineQueryFiltersAnd(string filter1, string filter2)
    {
        if (filter1.IsNullOrEmpty() || filter2.IsNullOrEmpty())
        {
            throw new ArgumentException("Provided filters should not be empty strings");
        }

        return $"({filter1}) and ({filter2})";
    }

    public static string CombineQueryFiltersOr(string filter1, string filter2)
    {
        if (filter1.IsNullOrEmpty() || filter2.IsNullOrEmpty())
        {
            throw new ArgumentException("Provided filters should not be empty strings");
        }

        return $"({filter1}) or ({filter2})";
    }

    /// <summary>
    /// Groups entities by PartitionKey into batches of max 100 for valid transactions.
    /// See <see href="https://learn.microsoft.com/en-us/rest/api/storageservices/performing-entity-group-transactions">here</see>.
    /// </summary>
    /// <returns>List of Azure Responses for Transactions</returns>
    internal static async Task<IReadOnlyList<Response<IReadOnlyList<Response>>>> BatchManipulateEntities<TEntity>(
        TableClient tableClient,
        IEnumerable<TEntity> entities,
        TableTransactionActionType tableTransactionActionType,
        CancellationToken cancellationToken = default) 
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
