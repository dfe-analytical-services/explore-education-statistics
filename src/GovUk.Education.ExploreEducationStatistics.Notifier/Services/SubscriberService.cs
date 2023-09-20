#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;
using GovUk.Education.ExploreEducationStatistics.Notifier.Services.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Notifier.Services;

public class SubscriberService : ISubscriberService
{
    private readonly TableServiceClient _tableServiceClient;

    public SubscriberService(TableServiceClient tableServiceClient)
    {
        _tableServiceClient = tableServiceClient;
    }

    public async Task CreateOrUpdateSubscriber(string tableName,
        SubscriptionEntity subscription,
        CancellationToken cancellationToken = default)
    {
        var tableClient = _tableServiceClient.GetTableClient(
            tableName: tableName
        );
        await tableClient.AddEntityAsync(subscription,
            cancellationToken: cancellationToken);
    }

    public async Task RemovePendingSubscriber(string publicationId,
        string email,
        CancellationToken cancellationToken = default)
    {
        var tableClient = _tableServiceClient.GetTableClient(
            tableName: SubscriptionTableNames.PendingSubscriptionsTableName
        );
        await tableClient.DeleteEntityAsync(partitionKey: publicationId,
            rowKey: email,
            cancellationToken: cancellationToken);
    }

    public async Task RemoveSubscriber(string publicationId,
        string email,
        CancellationToken cancellationToken = default)
    {
        var tableClient = _tableServiceClient.GetTableClient(
            tableName: SubscriptionTableNames.SubscriptionsTableName
        );
        await tableClient.DeleteEntityAsync(partitionKey: publicationId,
            rowKey: email,
            cancellationToken: cancellationToken);
    }

    public AsyncPageable<SubscriptionEntity> RetrieveSubscribers(Guid publicationId,
        CancellationToken cancellationToken = default)
    {
        var tableClient = _tableServiceClient.GetTableClient(
            tableName: SubscriptionTableNames.SubscriptionsTableName
        );

        // Remove any pending subscriptions where the token has expired i.e. more than 1 hour old
        return tableClient.QueryAsync<SubscriptionEntity>(
            filter: $"PartitionKey eq '{publicationId}'",
            maxPerPage: 10,
            cancellationToken: cancellationToken);
    }

    public async Task<SubscriptionEntity> RetrievePendingSubscriber(string publicationId, string email,
        CancellationToken cancellationToken = default)
    {
        var tableClient = _tableServiceClient.GetTableClient(
            tableName: SubscriptionTableNames.PendingSubscriptionsTableName
        );
        return await tableClient.GetEntityAsync<SubscriptionEntity>(partitionKey: publicationId,
            rowKey: email,
            select: new List<string>
            {
                "Slug", "Title"
            },
            cancellationToken: cancellationToken);
    }

    public async Task<SubscriptionEntity> RetrieveSubscriber(string publicationId,
        string email,
        CancellationToken cancellationToken = default)
    {
        var tableClient = _tableServiceClient.GetTableClient(
            tableName: SubscriptionTableNames.SubscriptionsTableName
        );
        return await tableClient.GetEntityAsync<SubscriptionEntity>(partitionKey: publicationId,
            rowKey: email,
            select: new List<string>
            {
                "Slug", "Title"
            },
            cancellationToken: cancellationToken);
    }

    public async Task RemoveExpiredPendingSubscriptions(CancellationToken cancellationToken = default)
    {
        var tableClient = _tableServiceClient.GetTableClient(
            tableName: SubscriptionTableNames.PendingSubscriptionsTableName
        );

        // Remove any pending subscriptions where the token has expired i.e. more than 1 hour old

        var queryResultsMaxPerPage = tableClient.QueryAsync<SubscriptionEntity>(
            filter: $"DateTimeCreated lt {DateTime.UtcNow.AddHours(1)}",
            maxPerPage: 10,
            cancellationToken: cancellationToken);

        await foreach (var page in queryResultsMaxPerPage.AsPages().WithCancellation(cancellationToken))
        {
            foreach (var subscription in page.Values)
            {
                await tableClient.DeleteEntityAsync(partitionKey: subscription.PartitionKey,
                    rowKey: subscription.RowKey,
                    cancellationToken: cancellationToken);
            }
        }
    }
}
