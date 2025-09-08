using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;
using GovUk.Education.ExploreEducationStatistics.Notifier.Model;

namespace GovUk.Education.ExploreEducationStatistics.Notifier.Repositories.Interfaces;

public interface IApiSubscriptionRepository
{
    Task<ApiSubscription?> GetSubscription(Guid dataSetId, string email, CancellationToken cancellationToken = default);

    Task CreatePendingSubscription(
        Guid dataSetId,
        string dataSetTitle,
        string email,
        DateTimeOffset expiryDateTime,
        CancellationToken cancellationToken = default
    );

    Task UpdateSubscription(ApiSubscription subscription, CancellationToken cancellationToken = default);

    Task DeleteSubscription(Guid dataSetId, string email, CancellationToken cancellationToken = default);

    Task<AsyncPageable<ApiSubscription>> QuerySubscriptions(
        Expression<Func<ApiSubscription, bool>>? filter = null,
        int? maxPerPage = 1000,
        IEnumerable<string>? select = null,
        CancellationToken cancellationToken = default
    );

    Task BatchManipulateSubscriptions(
        IEnumerable<ApiSubscription> subscriptions,
        TableTransactionActionType tableTransactionActionType,
        CancellationToken cancellationToken = default
    );
}
