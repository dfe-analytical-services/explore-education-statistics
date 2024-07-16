using System;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Notifier.Configuration;
using GovUk.Education.ExploreEducationStatistics.Notifier.Model;
using GovUk.Education.ExploreEducationStatistics.Notifier.Repositories.Interfaces;
using Microsoft.Extensions.Options;

namespace GovUk.Education.ExploreEducationStatistics.Notifier.Repositories;

internal class ApiSubscriptionRepository(
    IOptions<AppSettingsOptions> appSettingsOptions,
    IApiSubscriptionTableStorageService apiSubscriptionTableStorage) : IApiSubscriptionRepository
{
    private readonly string _apiSubscriptionsTableName = appSettingsOptions.Value.ApiSubscriptionsTableName;

    public async Task<ApiSubscription?> GetSubscription(
        Guid dataSetId,
        string email,
        CancellationToken cancellationToken = default)
    {
        return await apiSubscriptionTableStorage.GetEntityIfExists<ApiSubscription>(
            tableName: _apiSubscriptionsTableName,
            partitionKey: email,
            rowKey: dataSetId.ToString(),
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
            PartitionKey = email,
            RowKey = dataSetId.ToString(),
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
            partitionKey: email,
            rowKey: dataSetId.ToString(),
            cancellationToken: cancellationToken);
    }
}
