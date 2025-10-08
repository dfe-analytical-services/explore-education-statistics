using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Notifier.Model;
using GovUk.Education.ExploreEducationStatistics.Notifier.Requests;
using GovUk.Education.ExploreEducationStatistics.Notifier.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using FromBodyAttribute = Microsoft.Azure.Functions.Worker.Http.FromBodyAttribute;

namespace GovUk.Education.ExploreEducationStatistics.Notifier.Functions;

public class ApiSubscriptionFunctions(
    ILogger<ApiSubscriptionFunctions> logger,
    IApiSubscriptionService apiSubscriptionService,
    IValidator<PendingApiSubscriptionCreateRequest> newPendingApiSubscriptionRequestValidator,
    IValidator<ApiNotificationMessage> apiNotificationMessageValidator
)
{
    private static class FunctionNames
    {
        private const string Base = "PublicApiSubscriptions_";
        public const string NotifySubscribers = $"{Base}{nameof(NotifySubscribers)}";
        public const string RequestPendingSubscription = $"{Base}{nameof(RequestPendingSubscription)}";
        public const string RemoveExpiredSubscriptions = $"{Base}{nameof(RemoveExpiredSubscriptions)}";
        public const string Unsubscribe = $"{Base}{nameof(Unsubscribe)}";
        public const string VerifySubscription = $"{Base}{nameof(VerifySubscription)}";
    }

    [Function(FunctionNames.RequestPendingSubscription)]
    public async Task<IActionResult> RequestPendingSubscription(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "public-api/request-pending-subscription")]
        [FromBody]
            PendingApiSubscriptionCreateRequest request,
        CancellationToken cancellationToken
    )
    {
        try
        {
            return await newPendingApiSubscriptionRequestValidator
                .Validate(request, cancellationToken)
                .OnSuccess(() =>
                    apiSubscriptionService.RequestPendingSubscription(
                        dataSetId: request.DataSetId,
                        dataSetTitle: request.DataSetTitle,
                        email: request.Email,
                        cancellationToken: cancellationToken
                    )
                )
                .HandleFailuresOr(subscription => new OkObjectResult(subscription));
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Exception occured while executing '{FunctionName}'",
                nameof(RequestPendingSubscription)
            );
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }

    [Function(FunctionNames.VerifySubscription)]
    public async Task<IActionResult> VerifySubscription(
        [HttpTrigger(
            AuthorizationLevel.Anonymous,
            "post",
            Route = "public-api/{dataSetId:guid}/verify-subscription/{token}"
        )]
            HttpRequest request,
        Guid dataSetId,
        string token,
        CancellationToken cancellationToken
    )
    {
        try
        {
            return await apiSubscriptionService
                .VerifySubscription(dataSetId: dataSetId, token: token, cancellationToken: cancellationToken)
                .HandleFailuresOr(subscription => new OkObjectResult(subscription));
        }
        catch (Exception ex)
        {
            logger.LogError(
                exception: ex,
                "Exception occured while executing '{FunctionName}'",
                nameof(VerifySubscription)
            );
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }

    [Function(FunctionNames.Unsubscribe)]
    public async Task<IActionResult> Unsubscribe(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "public-api/{dataSetId:guid}/unsubscribe/{token}")]
            HttpRequest request,
        Guid dataSetId,
        string token,
        CancellationToken cancellationToken
    )
    {
        try
        {
            return await apiSubscriptionService
                .Unsubscribe(dataSetId: dataSetId, token: token, cancellationToken: cancellationToken)
                .HandleFailuresOrNoContent(convertNotFoundToNoContent: false);
        }
        catch (Exception ex)
        {
            logger.LogError(exception: ex, "Exception occured while executing '{FunctionName}'", nameof(Unsubscribe));
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }

    [Function(FunctionNames.RemoveExpiredSubscriptions)]
    public async Task RemoveExpiredSubscriptions(
        [TimerTrigger("0 * * * * *")] TimerInfo timerInfo,
        CancellationToken cancellationToken
    )
    {
        await apiSubscriptionService.RemoveExpiredApiSubscriptions(cancellationToken);
    }

    [Function(FunctionNames.NotifySubscribers)]
    public async Task NotifySubscribers(
        [QueueTrigger(NotifierQueueStorage.ApiNotificationQueue)] ApiNotificationMessage message,
        CancellationToken cancellationToken
    )
    {
        await apiNotificationMessageValidator.ValidateAndThrowAsync(message, cancellationToken);
        await apiSubscriptionService.NotifyApiSubscribers(
            dataSetId: message.DataSetId,
            dataSetFileId: message.DataSetFileId,
            version: message.Version,
            cancellationToken: cancellationToken
        );
    }
}
