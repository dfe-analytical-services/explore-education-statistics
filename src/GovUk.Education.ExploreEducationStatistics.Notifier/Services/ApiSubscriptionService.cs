using Azure;
using Azure.Data.Tables;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.Validators.ErrorDetails;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Notifier.Configuration;
using GovUk.Education.ExploreEducationStatistics.Notifier.Model;
using GovUk.Education.ExploreEducationStatistics.Notifier.Repositories.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Notifier.Requests;
using GovUk.Education.ExploreEducationStatistics.Notifier.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Notifier.Validators.ErrorDetails;
using GovUk.Education.ExploreEducationStatistics.Notifier.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using ValidationMessages = GovUk.Education.ExploreEducationStatistics.Notifier.Validators.ValidationMessages;

namespace GovUk.Education.ExploreEducationStatistics.Notifier.Services;

internal class ApiSubscriptionService(
    IOptions<AppSettingsOptions> appSettingsOptions,
    IOptions<GovUkNotifyOptions> govUkNotifyOptions,
    ITokenService tokenService,
    IEmailService emailService,
    IApiSubscriptionRepository apiSubscriptionRepository) : IApiSubscriptionService
{
    public async Task<Either<ActionResult, ApiSubscriptionViewModel>> RequestPendingSubscription(
        Guid dataSetId,
        string dataSetTitle,
        string email,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await HandleIfResubmission(
                    dataSetId: dataSetId,
                    email: email,
                    cancellationToken: cancellationToken)
                .OnSuccessVoid(_ => CreatePendingSubscription(
                    dataSetId: dataSetId,
                    dataSetTitle: dataSetTitle,
                    email: email,
                    cancellationToken: cancellationToken))
                .OnSuccess(_ => GetSubscription(
                    dataSetId: dataSetId,
                    email: email,
                    cancellationToken: cancellationToken))
                .OnSuccessDo(SendVerificationEmail)
                .OnSuccess(MapSubscription);
        }
        catch
        {
            await Rollback(
                dataSetId: dataSetId,
                email: email,
                cancellationToken: cancellationToken);

            throw;
        }
    }

    public async Task<Either<ActionResult, ApiSubscriptionViewModel>> VerifySubscription(
        Guid dataSetId,
        string token,
        CancellationToken cancellationToken = default)
    {
        return await GetEmailFromToken(token)
            .OnSuccessCombineWith(email => GetSubscription(
                dataSetId: dataSetId,
                email: email,
                cancellationToken: cancellationToken))
            .OnSuccess(async tuple =>
            {
                var (email, subscription) = tuple;

                return await VerifySubscription(subscription, cancellationToken)
                    .OnSuccess(_ => (email, subscription));
            })
            .OnSuccess(tuple => GetSubscription(
                dataSetId: dataSetId,
                email: tuple.email,
                cancellationToken: cancellationToken))
            .OnSuccessDo(SendConfirmationEmail)
            .OnSuccess(MapSubscription);
    }

    public async Task<Either<ActionResult, Unit>> Unsubscribe(
        Guid dataSetId,
        string token,
        CancellationToken cancellationToken = default)
    {
        return await GetEmailFromToken(token)
            .OnSuccess(email => GetSubscription(
                dataSetId: dataSetId,
                email: email,
                cancellationToken: cancellationToken))
            .OnSuccess(subscription => DeleteSubscription(subscription: subscription, cancellationToken: cancellationToken));
    }

    public async Task RemoveExpiredApiSubscriptions(CancellationToken cancellationToken = default)
    {
        var expiredSubscriptions = await GetExpiredApiSubscriptions(cancellationToken);

        await expiredSubscriptions
            .AsPages()
            .ForEachAwaitAsync(
                async page => await BatchDeleteApiSubscriptions(page, cancellationToken),
                cancellationToken);
    }

    private async Task<Either<ActionResult, Unit>> HandleIfResubmission(
        Guid dataSetId,
        string email,
        CancellationToken cancellationToken)
    {
        var subscription = await GetSubscription(
            dataSetId: dataSetId,
            email: email,
            cancellationToken: cancellationToken);

        return subscription.IsLeft
            ? (
                subscription.Left is NotFoundResult
                ? Unit.Instance
                : subscription.Left
            )
            : (
                subscription.Right.Status is ApiSubscriptionStatus.SubscriptionPending
                ? ValidationUtils.ValidationResult(new ErrorViewModel
                {
                    Code = ValidationMessages.ApiPendingSubscriptionAlreadyExists.Code,
                    Message = ValidationMessages.ApiPendingSubscriptionAlreadyExists.Message,
                    Detail = new ApiSubscriptionErrorDetail(dataSetId, email),
                    Path = nameof(PendingApiSubscriptionCreateRequest.DataSetId).ToLowerFirst()
                })
                : ValidationUtils.ValidationResult(new ErrorViewModel
                {
                    Code = ValidationMessages.ApiVerifiedSubscriptionAlreadyExists.Code,
                    Message = ValidationMessages.ApiVerifiedSubscriptionAlreadyExists.Message,
                    Detail = new ApiSubscriptionErrorDetail(dataSetId, email),
                    Path = nameof(PendingApiSubscriptionCreateRequest.DataSetId).ToLowerFirst()
                })
            );
    }

    private async Task<Either<ActionResult, ApiSubscription>> GetSubscription(
    Guid dataSetId,
    string email,
    CancellationToken cancellationToken)
    {
        var subscription = await apiSubscriptionRepository.GetSubscription(
            dataSetId: dataSetId,
            email: email,
            cancellationToken: cancellationToken);

        return subscription is null
            ? new NotFoundResult()
            : subscription;
    }

    private async Task<Either<ActionResult, Unit>> CreatePendingSubscription(
        Guid dataSetId,
        string dataSetTitle,
        string email,
        CancellationToken cancellationToken)
    {
        var expiryDateTime = DateTime.UtcNow.AddHours(1);

        await apiSubscriptionRepository.CreatePendingSubscription(
            dataSetId: dataSetId,
            dataSetTitle: dataSetTitle,
            email: email,
            expiryDateTime: expiryDateTime,
            cancellationToken: cancellationToken);

        return Unit.Instance;
    }

    private Either<ActionResult, string> GetEmailFromToken(string token)
    {
        var email = tokenService.GetEmailFromToken(token);

        return email is null
            ? ValidationUtils.ValidationResult(new ErrorViewModel
            {
                Code = ValidationMessages.AuthorizationTokenInvalid.Code,
                Message = ValidationMessages.AuthorizationTokenInvalid.Message,
                Detail = new InvalidErrorDetail<string>(token),
                Path = "token"
            })
            : email;
    }

    private async Task<Either<ActionResult, Unit>> VerifySubscription(ApiSubscription subscription, CancellationToken cancellationToken)
    {
        if (subscription.Status is ApiSubscriptionStatus.Subscribed)
        {
            return ValidationUtils.ValidationResult(new ErrorViewModel
            {
                Code = ValidationMessages.ApiVerifiedSubscriptionAlreadyExists.Code,
                Message = ValidationMessages.ApiVerifiedSubscriptionAlreadyExists.Message,
                Detail = new ApiSubscriptionErrorDetail(Guid.Parse(subscription.PartitionKey), subscription.RowKey),
                Path = nameof(PendingApiSubscriptionCreateRequest.DataSetId).ToLowerFirst()
            });
        }

        if (subscription.HasExpired)
        {
            return ValidationUtils.ValidationResult(new ErrorViewModel
            {
                Code = ValidationMessages.ApiPendingSubscriptionAlreadyExpired.Code,
                Message = ValidationMessages.ApiPendingSubscriptionAlreadyExpired.Message,
                Detail = new ApiSubscriptionErrorDetail(Guid.Parse(subscription.PartitionKey), subscription.RowKey),
                Path = nameof(PendingApiSubscriptionCreateRequest.DataSetId).ToLowerFirst()
            });
        }

        subscription.Expiry = null;
        subscription.Status = ApiSubscriptionStatus.Subscribed;

        await apiSubscriptionRepository.UpdateSubscription(subscription, cancellationToken);

        return Unit.Instance;
    }

    private void SendVerificationEmail(ApiSubscription subscription)
    {
        var activationCode = tokenService.GenerateToken(subscription.RowKey, subscription.Expiry!.Value.UtcDateTime);

        var emailTemplateVariables = new Dictionary<string, dynamic>
        {
            { "api_dataset", subscription.DataSetTitle },
            {
                "verification_link",
                $"{appSettingsOptions.Value.PublicAppUrl}/api-subscriptions/{subscription.PartitionKey}/confirm-subscription/{activationCode}"
            }
        };

        emailService.SendEmail(
            email: subscription.RowKey,
            templateId: govUkNotifyOptions.Value.EmailTemplates.ApiSubscriptionVerificationId,
            values: emailTemplateVariables);
    }

    private void SendConfirmationEmail(ApiSubscription subscription)
    {
        var expiryDateTime = DateTime.UtcNow.AddYears(1);
        var unsubscribeToken = tokenService.GenerateToken(subscription.RowKey, expiryDateTime);

        var emailTemplateVariables = new Dictionary<string, dynamic>
        {
            { "api_dataset", subscription.DataSetTitle },
            {
                "unsubscribe_link",
                $"{appSettingsOptions.Value.PublicAppUrl}/api-subscriptions/{subscription.PartitionKey}/confirm-unsubscription/{unsubscribeToken}"
            }
        };

        emailService.SendEmail(
            email: subscription.RowKey,
            templateId: govUkNotifyOptions.Value.EmailTemplates.ApiSubscriptionConfirmationId,
            values: emailTemplateVariables);
    }

    private static ApiSubscriptionViewModel MapSubscription(ApiSubscription subscription)
    {
        return new ApiSubscriptionViewModel
        {
            DataSetId = Guid.Parse(subscription.PartitionKey),
            DataSetTitle = subscription.DataSetTitle,
            Email = subscription.RowKey,
            Status = subscription.Status
        };
    }

    private async Task Rollback(Guid dataSetId, string email, CancellationToken cancellationToken)
    {
        await apiSubscriptionRepository.DeleteSubscription(
            dataSetId: dataSetId,
            email: email,
            cancellationToken: cancellationToken);
    }

    private async Task<Either<ActionResult, Unit>> DeleteSubscription(
        ApiSubscription subscription,
        CancellationToken cancellationToken = default)
    {
        if (subscription.Status is not ApiSubscriptionStatus.Subscribed)
        {
            return ValidationUtils.ValidationResult(new ErrorViewModel
            {
                Code = ValidationMessages.ApiSubscriptionHasNotBeenVerified.Code,
                Message = ValidationMessages.ApiSubscriptionHasNotBeenVerified.Message,
                Detail = new ApiSubscriptionErrorDetail(Guid.Parse(subscription.PartitionKey), subscription.RowKey),
                Path = nameof(PendingApiSubscriptionCreateRequest.DataSetId).ToLowerFirst()
            });
        }

        await apiSubscriptionRepository.DeleteSubscription(
            dataSetId: Guid.Parse(subscription.PartitionKey),
            email: subscription.RowKey,
            cancellationToken: cancellationToken);

        return Unit.Instance;
    }

    private async Task<AsyncPageable<ApiSubscription>> GetExpiredApiSubscriptions(CancellationToken cancellationToken)
    {
        Expression<Func<ApiSubscription, bool>> filter = s =>
            s.Status.Equals(ApiSubscriptionStatus.SubscriptionPending.ToString())
            && s.Expiry <= DateTimeOffset.UtcNow;

        return await apiSubscriptionRepository.QuerySubscriptions(
            filter: filter,
            select: new List<string>() { nameof(ApiSubscription.PartitionKey), nameof(ApiSubscription.RowKey) },
            cancellationToken: cancellationToken);
    }

    private async Task BatchDeleteApiSubscriptions(
        Page<ApiSubscription> subscriptions,
        CancellationToken cancellationToken)
    {
        await apiSubscriptionRepository.BatchManipulateSubscriptions(
            subscriptions: subscriptions.Values,
            tableTransactionActionType: TableTransactionActionType.Delete,
            cancellationToken: cancellationToken);
    }
}
