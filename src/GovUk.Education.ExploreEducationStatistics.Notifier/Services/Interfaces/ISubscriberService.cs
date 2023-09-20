#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using Azure;

namespace GovUk.Education.ExploreEducationStatistics.Notifier.Services.Interfaces;

public interface ISubscriberService
{
    Task CreateOrUpdateSubscriber(string tableName,
        SubscriptionEntity subscription,
        CancellationToken cancellationToken = default);

    Task RemoveExpiredPendingSubscriptions(CancellationToken cancellationToken = default);

    Task RemovePendingSubscriber(string publicationId,
        string email,
        CancellationToken cancellationToken = default);

    Task RemoveSubscriber(string publicationId,
        string email,
        CancellationToken cancellationToken = default);

    Task<SubscriptionEntity?> RetrievePendingSubscriber(string publicationId,
        string email,
        CancellationToken cancellationToken = default);

    AsyncPageable<SubscriptionEntity> RetrieveSubscribers(
        Guid publicationId,
        CancellationToken cancellationToken = default);

    Task<SubscriptionEntity?> RetrieveSubscriber(string publicationId,
        string email,
        CancellationToken cancellationToken = default);
}
