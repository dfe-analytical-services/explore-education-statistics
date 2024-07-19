using System;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Notifier.Model;

namespace GovUk.Education.ExploreEducationStatistics.Notifier.Repositories.Interfaces;

public interface IApiSubscriptionRepository
{
    Task<ApiSubscription?> GetSubscription(
        Guid dataSetId, 
        string email, 
        CancellationToken cancellationToken = default);

    Task CreatePendingSubscription(
        Guid dataSetId,
        string dataSetTitle,
        string email, 
        DateTimeOffset expiryDateTime,
        CancellationToken cancellationToken = default);

    Task UpdateSubscription(
        ApiSubscription subscription,
        CancellationToken cancellationToken = default);

    Task DeleteSubscription(
        Guid dataSetId, 
        string email,
        CancellationToken cancellationToken = default);
}
