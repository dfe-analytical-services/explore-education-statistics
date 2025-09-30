using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;
using GovUk.Education.ExploreEducationStatistics.Notifier.Model;
using GovUk.Education.ExploreEducationStatistics.Notifier.Repositories.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Notifier.Services.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Notifier.Repositories;

internal class ApiSubscriptionRepository(INotifierTableStorageService notifierTableStorage)
    : IApiSubscriptionRepository
{
    public async Task<ApiSubscription?> GetSubscription(
        Guid dataSetId,
        string email,
        CancellationToken cancellationToken = default
    )
    {
        return await notifierTableStorage.GetEntityIfExists<ApiSubscription>(
            tableName: NotifierTableStorage.ApiSubscriptionsTable,
            partitionKey: dataSetId.ToString(),
            rowKey: email,
            cancellationToken: cancellationToken
        );
    }

    public async Task CreatePendingSubscription(
        Guid dataSetId,
        string dataSetTitle,
        string email,
        DateTimeOffset expiry,
        CancellationToken cancellationToken = default
    )
    {
        var subscription = new ApiSubscription
        {
            PartitionKey = dataSetId.ToString(),
            RowKey = email,
            DataSetTitle = dataSetTitle,
            Status = ApiSubscriptionStatus.Pending,
            Expiry = expiry,
        };

        await notifierTableStorage.CreateEntity(
            tableName: NotifierTableStorage.ApiSubscriptionsTable,
            entity: subscription,
            cancellationToken: cancellationToken
        );
    }

    public async Task UpdateSubscription(
        ApiSubscription subscription,
        CancellationToken cancellationToken = default
    )
    {
        await notifierTableStorage.UpdateEntity(
            tableName: NotifierTableStorage.ApiSubscriptionsTable,
            entity: subscription,
            cancellationToken: cancellationToken
        );
    }

    public async Task DeleteSubscription(
        Guid dataSetId,
        string email,
        CancellationToken cancellationToken = default
    )
    {
        await notifierTableStorage.DeleteEntity(
            tableName: NotifierTableStorage.ApiSubscriptionsTable,
            partitionKey: dataSetId.ToString(),
            rowKey: email,
            cancellationToken: cancellationToken
        );
    }

    public async Task<AsyncPageable<ApiSubscription>> QuerySubscriptions(
        Expression<Func<ApiSubscription, bool>>? filter = null,
        int? maxPerPage = 1000,
        IEnumerable<string>? select = null,
        CancellationToken cancellationToken = default
    )
    {
        filter ??= subscription => true;

        return await notifierTableStorage.QueryEntities(
            tableName: NotifierTableStorage.ApiSubscriptionsTable,
            filter: filter,
            maxPerPage: maxPerPage,
            select: select,
            cancellationToken: cancellationToken
        );
    }

    public async Task BatchManipulateSubscriptions(
        IEnumerable<ApiSubscription> subscriptions,
        TableTransactionActionType tableTransactionActionType,
        CancellationToken cancellationToken = default
    )
    {
        await notifierTableStorage.BatchManipulateEntities(
            tableName: NotifierTableStorage.ApiSubscriptionsTable,
            entities: subscriptions,
            tableTransactionActionType: tableTransactionActionType,
            cancellationToken: cancellationToken
        );
    }
}
