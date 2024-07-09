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
using System.Threading;
using System.Threading.Tasks;
using ValidationMessages = GovUk.Education.ExploreEducationStatistics.Notifier.Validators.ValidationMessages;

namespace GovUk.Education.ExploreEducationStatistics.Notifier.Services;

internal class ApiSubscriptionService(
    IOptions<AppSettingsOptions> appSettingsOptions,
    IOptions<GovUkNotifyOptions> govUkNotifyOptions,
    ITokenService tokenService,
    IEmailService emailService,
    IApiSubscriptionRepository apiSubscriptionRepository,
    INotificationClientProvider notificationClientProvider) : IApiSubscriptionService
{
    public async Task<Either<ActionResult, ApiSubscriptionViewModel>> RequestPendingSubscription(
        Guid dataSetId,
        string dataSetTitle,
        string email,
        CancellationToken cancellationToken)
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
        CancellationToken cancellationToken)
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
                    Path = nameof(NewPendingApiSubscriptionRequest.DataSetId).ToLowerFirst()
                })
                : ValidationUtils.ValidationResult(new ErrorViewModel
                {
                    Code = ValidationMessages.ApiVerifiedSubscriptionAlreadyExists.Code,
                    Message = ValidationMessages.ApiVerifiedSubscriptionAlreadyExists.Message,
                    Detail = new ApiSubscriptionErrorDetail(dataSetId, email),
                    Path = nameof(NewPendingApiSubscriptionRequest.DataSetId).ToLowerFirst()
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
        subscription.ExpiryDateTime = null;
        subscription.Status = ApiSubscriptionStatus.Subscribed;

        await apiSubscriptionRepository.UpdateSubscription(subscription, cancellationToken);

        return Unit.Instance;
    }

    private void SendVerificationEmail(ApiSubscription subscription)
    {
        var activationCode = tokenService.GenerateToken(subscription.RowKey, subscription.ExpiryDateTime!.Value.UtcDateTime);

        var emailTemplateVariables = new Dictionary<string, dynamic>
        {
            { "api_dataset", subscription.DataSetTitle },
            {
                "verification_link",
                $"{appSettingsOptions.Value.PublicAppUrl}/api-subscriptions/{subscription.PartitionKey}/confirm-subscription/{activationCode}"
            }
        };

        SendEmail(
            email: subscription.RowKey,
            emailTemplateId: govUkNotifyOptions.Value.EmailTemplates.ApiSubscriptionVerificationId,
            emailTemplateVariables: emailTemplateVariables);
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

        SendEmail(
            email: subscription.RowKey,
            emailTemplateId: govUkNotifyOptions.Value.EmailTemplates.ApiSubscriptionConfirmationId,
            emailTemplateVariables: emailTemplateVariables);
    }

    private void SendEmail(
        string email,
        string emailTemplateId,
        Dictionary<string, dynamic> emailTemplateVariables)
    {
        var notificationClient = notificationClientProvider.Get();

        emailService.SendEmail(
            client: notificationClient,
            email: email,
            templateId: emailTemplateId,
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
}
