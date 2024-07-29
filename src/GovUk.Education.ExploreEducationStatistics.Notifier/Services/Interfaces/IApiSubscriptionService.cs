using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Notifier.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Notifier.Services.Interfaces;

public interface IApiSubscriptionService
{
    Task<Either<ActionResult, ApiSubscriptionViewModel>> RequestPendingSubscription(
        Guid dataSetId,
        string dataSetTitle,
        string email,
        CancellationToken cancellationToken = default);

    Task<Either<ActionResult, ApiSubscriptionViewModel>> VerifySubscription(
        Guid dataSetId,
        string token,
        CancellationToken cancellationToken = default);

    Task<Either<ActionResult, Unit>> Unsubscribe(
        Guid dataSetId,
        string token,
        CancellationToken cancellationToken = default);

    Task RemoveExpiredApiSubscriptions(CancellationToken cancellationToken = default);
}
