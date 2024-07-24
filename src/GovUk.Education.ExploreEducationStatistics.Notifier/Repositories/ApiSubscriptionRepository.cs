using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;
using GovUk.Education.ExploreEducationStatistics.Notifier.Configuration;
using GovUk.Education.ExploreEducationStatistics.Notifier.Model;
using GovUk.Education.ExploreEducationStatistics.Notifier.Repositories.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Notifier.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace GovUk.Education.ExploreEducationStatistics.Notifier.Repositories;

internal class ApiSubscriptionRepository(
    IOptions<AppSettingsOptions> appSettingsOptions,
    IApiSubscriptionTableStorageService apiSubscriptionTableStorage) : IApiSubscriptionRepository
{
    private const string _apiSubscriptionsTableName = Constants.NotifierTableStorageTableNames.ApiSubscriptionsTableName;

    public async Task<ApiSubscription?> GetSubscription(
        Guid dataSetId,
        string email,
        CancellationToken cancellationToken = default)
    {
        return await apiSubscriptionTableStorage.GetEntityIfExists<ApiSubscription>(
            tableName: _apiSubscriptionsTableName,
            partitionKey: dataSetId.ToString(),
            rowKey: email,
            cancellationToken: cancellationToken);
    }

    public async Task CreatePendingSubscription(
        Guid dataSetId,
        string dataSetTitle,
        string email,
        DateTimeOffset expiry,
        CancellationToken cancellationToken = default)
    {
        var subscription = new ApiSubscription
        {
            PartitionKey = dataSetId.ToString(),
            RowKey = email,
            DataSetTitle = dataSetTitle,
            Status = ApiSubscriptionStatus.SubscriptionPending,
            Expiry = expiry
        };

        await apiSubscriptionTableStorage.CreateEntity(
            tableName: _apiSubscriptionsTableName,
            entity: subscription,
            cancellationToken: cancellationToken);
    }

    public async Task UpdateSubscription(
        ApiSubscription subscription,
        CancellationToken cancellationToken = default)
    {
        await apiSubscriptionTableStorage.UpdateEntity(
            tableName: _apiSubscriptionsTableName,
            entity: subscription,
            cancellationToken: cancellationToken);
    }

    public async Task DeleteSubscription(
        Guid dataSetId,
        string email,
        CancellationToken cancellationToken = default)
    {
        await apiSubscriptionTableStorage.DeleteEntity(
            tableName: _apiSubscriptionsTableName,
            partitionKey: dataSetId.ToString(),
            rowKey: email,
            cancellationToken: cancellationToken);
    }

    public async Task<AsyncPageable<ApiSubscription>> QuerySubscriptions(
        Expression<Func<ApiSubscription, bool>>? filter = null, 
        int? maxPerPage = 1000, 
        IEnumerable<string>? select = null, 
        CancellationToken cancellationToken = default)
    {
        filter ??= subscription => true;

        return await apiSubscriptionTableStorage.QueryEntities(
            tableName: _apiSubscriptionsTableName,
            filter: filter,
            maxPerPage: maxPerPage,
            select: select,
            cancellationToken: cancellationToken);
    }

    public async Task BatchManipulateSubscriptions(
        IEnumerable<ApiSubscription> subscriptions,
        TableTransactionActionType tableTransactionActionType, 
        CancellationToken cancellationToken = default)
    {
        await apiSubscriptionTableStorage.BatchManipulateEntities(
            tableName: _apiSubscriptionsTableName,
            entities: subscriptions,
            tableTransactionActionType: tableTransactionActionType,
            cancellationToken: cancellationToken);
    }
}
