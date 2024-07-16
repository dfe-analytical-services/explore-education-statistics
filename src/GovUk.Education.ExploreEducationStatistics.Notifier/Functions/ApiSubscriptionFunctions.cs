using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Notifier.Requests;
using FromBodyAttribute = Microsoft.Azure.Functions.Worker.Http.FromBodyAttribute;
using GovUk.Education.ExploreEducationStatistics.Notifier.Services.Interfaces;
using System;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using FluentValidation;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

namespace GovUk.Education.ExploreEducationStatistics.Notifier.Functions;

public class ApiSubscriptionFunctions(
    ILogger<ApiSubscriptionFunctions> logger,
    IApiSubscriptionService apiSubscriptionService,
    IValidator<PendingApiSubscriptionCreateRequest> newPendingApiSubscriptionRequestValidator)
{
    [Function("RequestPendingApiSubscription")]
    public async Task<IActionResult> RequestPendingApiSubscription(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "public-api/request-pending-subscription/")]
        [FromBody] PendingApiSubscriptionCreateRequest request,
        FunctionContext context,
        CancellationToken cancellationToken)
    {
        try
        {
            return await newPendingApiSubscriptionRequestValidator.Validate(request, cancellationToken)
                .OnSuccess(() => apiSubscriptionService.RequestPendingSubscription(
                    dataSetId: request.DataSetId,
                    dataSetTitle: request.DataSetTitle,
                    email: request.Email,
                    cancellationToken: cancellationToken))
                .HandleFailuresOr(subscription => new OkObjectResult(subscription));
        }
        catch (Exception ex)
        {
            logger.LogError(exception: ex, "Exception occured while executing '{FunctionName}'", nameof(RequestPendingApiSubscription));
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }

    [Function("VerifyApiSubscription")]
    public async Task<IActionResult> VerifyApiSubscription(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "public-api/{dataSetId:guid}/verify-subscription/{token}")]
        FunctionContext context,
        Guid dataSetId,
        string token,
        CancellationToken cancellationToken)
    {
        try
        {
            return await apiSubscriptionService.VerifySubscription(
                dataSetId: dataSetId,
                token: token,
                cancellationToken: cancellationToken)
                .HandleFailuresOr(subscription => new OkObjectResult(subscription));
        }
        catch (Exception ex)
        {
            logger.LogError(exception: ex, "Exception occured while executing '{FunctionName}'", nameof(VerifyApiSubscription));
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }

    [Function("RemoveExpiredApiSubscriptions")]
    public async Task RemoveExpiredApiSubscriptions(
        [TimerTrigger("0 0 * * * *")] TimerInfo timerInfo,
        FunctionContext context,
        CancellationToken cancellationToken)
    {
        await apiSubscriptionService.RemoveExpiredApiSubscriptions(cancellationToken);
    }
}
