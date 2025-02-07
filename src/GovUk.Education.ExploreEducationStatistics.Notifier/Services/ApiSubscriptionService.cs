using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.Validators.ErrorDetails;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Notifier.Options;
using GovUk.Education.ExploreEducationStatistics.Notifier.Model;
using GovUk.Education.ExploreEducationStatistics.Notifier.Repositories.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Notifier.Requests;
using GovUk.Education.ExploreEducationStatistics.Notifier.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Notifier.Validators.ErrorDetails;
using GovUk.Education.ExploreEducationStatistics.Notifier.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ValidationMessages = GovUk.Education.ExploreEducationStatistics.Notifier.Validators.ValidationMessages;

namespace GovUk.Education.ExploreEducationStatistics.Notifier.Services;

internal class ApiSubscriptionService(
    IOptions<AppOptions> appOptions,
    IOptions<GovUkNotifyOptions> govUkNotifyOptions,
    ITokenService tokenService,
    IEmailService emailService,
    IApiSubscriptionRepository apiSubscriptionRepository) : IApiSubscriptionService
{
    private readonly string _publicAppUrl = appOptions.Value.PublicAppUrl;

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
            .OnSuccess(subscription =>
                DeleteSubscription(subscription: subscription, cancellationToken: cancellationToken));
    }

    public async Task NotifyApiSubscribers(
        Guid dataSetId,
        Guid dataSetFileId,
        string version,
        CancellationToken cancellationToken = default)
    {
        Expression<Func<ApiSubscription, bool>> filter = s =>
            s.PartitionKey == dataSetId.ToString()
            && s.Status.Equals(ApiSubscriptionStatus.Subscribed.ToString());

        var dataSetSubscribers = await apiSubscriptionRepository.QuerySubscriptions(
            filter: filter,
            select:
            [
                nameof(ApiSubscription.PartitionKey),
                nameof(ApiSubscription.RowKey),
                nameof(ApiSubscription.DataSetTitle)
            ],
            cancellationToken: cancellationToken);

        await dataSetSubscribers
            .AsPages()
            .ForEachAsync(
                page => BatchNotifySubscribers(
                    subscribers: page,
                    dataSetFileId: dataSetFileId,
                    version: version),
                cancellationToken);
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
            ? subscription.Left is NotFoundResult
                ? Unit.Instance
                : subscription.Left
            : subscription.Right.Status is ApiSubscriptionStatus.Pending
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
                });
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

    private async Task<Either<ActionResult, Unit>> VerifySubscription(
        ApiSubscription subscription, CancellationToken cancellationToken)
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
        var token = tokenService.GenerateToken(subscription.RowKey, subscription.Expiry!.Value.UtcDateTime);
        var verificationUrl =
            $"{_publicAppUrl}/api-subscriptions/{subscription.PartitionKey}/confirm-subscription/{token}";
        var personalisation = new Dictionary<string, dynamic>
        {
            { NotifierEmailTemplateFields.DataSetTitle, subscription.DataSetTitle },
            { NotifierEmailTemplateFields.VerificationUrl, verificationUrl }
        };

        emailService.SendEmail(
            email: subscription.RowKey,
            templateId: govUkNotifyOptions.Value.EmailTemplates.ApiSubscriptionVerificationId,
            values: personalisation);
    }

    private void SendConfirmationEmail(ApiSubscription subscription)
    {
        var token = tokenService.GenerateToken(subscription.RowKey, expiryDateTime: DateTime.UtcNow.AddYears(1));
        var unsubscribeUrl =
            $"{_publicAppUrl}/api-subscriptions/{subscription.PartitionKey}/confirm-unsubscription/{token}";
        var personalisation = new Dictionary<string, dynamic>
        {
            { NotifierEmailTemplateFields.DataSetTitle, subscription.DataSetTitle },
            { NotifierEmailTemplateFields.UnsubscribeUrl, unsubscribeUrl }
        };

        emailService.SendEmail(
            email: subscription.RowKey,
            templateId: govUkNotifyOptions.Value.EmailTemplates.ApiSubscriptionConfirmationId,
            values: personalisation);
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

    private void BatchNotifySubscribers(
        Page<ApiSubscription> subscribers,
        Guid dataSetFileId,
        string version)
    {
        foreach (var subscriber in subscribers.Values)
        {
            SendNotificationEmail(subscriber, dataSetFileId, version);
        }
    }

    private void SendNotificationEmail(
        ApiSubscription subscription,
        Guid dataSetFileId,
        string version)
    {
        var token = tokenService.GenerateToken(subscription.RowKey, expiryDateTime: DateTime.UtcNow.AddYears(1));
        var dataSetUrl = $"{_publicAppUrl}/data-catalogue/data-set/{dataSetFileId}";
        var unsubscribeUrl =
            $"{_publicAppUrl}/api-subscriptions/{subscription.PartitionKey}/confirm-unsubscription/{token}";
        var personalisation = new Dictionary<string, dynamic>
        {
            { NotifierEmailTemplateFields.DataSetTitle, subscription.DataSetTitle },
            { NotifierEmailTemplateFields.DataSetUrl, dataSetUrl },
            { NotifierEmailTemplateFields.DataSetVersion, version },
            { NotifierEmailTemplateFields.UnsubscribeUrl, unsubscribeUrl }
        };
        emailService.SendEmail(
            email: subscription.RowKey,
            templateId: govUkNotifyOptions.Value.EmailTemplates
                .SelectDataSetPublishedTemplateId(version),
            values: personalisation);
    }
    private async Task<AsyncPageable<ApiSubscription>> GetExpiredApiSubscriptions(CancellationToken cancellationToken)
    {
        Expression<Func<ApiSubscription, bool>> filter = s =>
            s.Status.Equals(ApiSubscriptionStatus.Pending.ToString())
            && s.Expiry <= DateTimeOffset.UtcNow;

        return await apiSubscriptionRepository.QuerySubscriptions(
            filter: filter,
            select:
            [
                nameof(ApiSubscription.PartitionKey),
                nameof(ApiSubscription.RowKey)
            ],
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
