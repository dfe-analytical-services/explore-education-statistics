using FluentValidation;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Notifier.Configuration;
using GovUk.Education.ExploreEducationStatistics.Notifier.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Notify.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.Data.Tables;
using GovUk.Education.ExploreEducationStatistics.Notifier.Requests;
using GovUk.Education.ExploreEducationStatistics.Notifier.Types;
using FromBodyAttribute = Microsoft.Azure.Functions.Worker.Http.FromBodyAttribute;
using GovUk.Education.ExploreEducationStatistics.Notifier.Repositories.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Notifier.Model;

namespace GovUk.Education.ExploreEducationStatistics.Notifier.Functions;

public class PublicationSubscriptionFunctions(
    ILogger<PublicationSubscriptionFunctions> logger,
    IOptions<AppSettingsOptions> appSettingsOptions,
    IOptions<GovUkNotifyOptions> govUkNotifyOptions,
    ITokenService tokenService,
    IEmailService emailService,
    IPublicationSubscriptionRepository publicationSubscriptionRepository,
    IValidator<PendingPublicationSubscriptionCreateRequest> requestValidator,
    IApiSubscriptionTableStorageService apiSubscriptionTableStorageService)
{
    private readonly AppSettingsOptions _appSettingsOptions = appSettingsOptions.Value;
    private readonly GovUkNotifyOptions.EmailTemplateOptions _emailTemplateOptions = govUkNotifyOptions.Value.EmailTemplates;

    private static class FunctionNames
    {
        private const string Base = "PublicationSubscriptions_";
        public const string RequestPendingSubscription = $"{Base}{nameof(PublicationSubscriptionFunctions.RequestPendingSubscription)}";
        public const string RemoveExpiredSubscriptions = $"{Base}{nameof(PublicationSubscriptionFunctions.RemoveExpiredSubscriptions)}";
        public const string Unsubscribe = $"{Base}{nameof(PublicationSubscriptionFunctions.Unsubscribe)}";
        public const string VerifySubscription = $"{Base}{nameof(PublicationSubscriptionFunctions.VerifySubscription)}";
    }

    [Function(FunctionNames.RequestPendingSubscription)]
    public async Task<IActionResult> RequestPendingSubscription(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "publication/request-pending-subscription/")]
        [FromBody] PendingPublicationSubscriptionCreateRequest req,
        FunctionContext context,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("{FunctionName} triggered", context.FunctionDefinition.Name);

        var validationResult = await requestValidator.Validate(req, cancellationToken);
        if (validationResult.IsLeft) // If validation failed
        {
            logger.LogError("{FunctionName} failed because the {req} did not contain a valid JSON object.", context.FunctionDefinition.Name, req);
            return validationResult.Left;
        }

        var subscription = await publicationSubscriptionRepository.GetSubscription(req.Id, req.Email);
        var pendingSubscriptionTable =
            await publicationSubscriptionRepository.GetTable(NotifierTableStorage.PublicationPendingSubscriptionsTable);

        try
        {
            logger.LogDebug("Pending subscription found?: {Status}", subscription.Status);


            switch (subscription.Status)
            {
                // If already existing and pending then don't send another one
                case SubscriptionStatus.SubscriptionPending:
                    return new OkResult();

                // Send confirmation email if user already subscribed
                case SubscriptionStatus.Subscribed:
                    {
                        var unsubscribeToken =
                            tokenService.GenerateToken(subscription.Subscriber.RowKey,
                                DateTime.UtcNow.AddYears(1));

                        var confirmationValues = new Dictionary<string, dynamic>
                        {
                            { "publication_name", subscription.Subscriber.Title },
                            {
                                "unsubscribe_link",
                                $"{_appSettingsOptions.PublicAppUrl}/subscriptions/{req.Slug}/confirm-unsubscription/{unsubscribeToken}"
                            }
                        };

                        emailService.SendEmail(
                            email: req.Email,
                            templateId: _emailTemplateOptions.SubscriptionConfirmationId,
                            confirmationValues);

                        return new OkResult();
                    }

                case SubscriptionStatus.NotSubscribed:
                    // Verification Token expires in 1 hour
                    var expiryDateTime = DateTime.UtcNow.AddHours(1);
                    var activationCode = tokenService.GenerateToken(req.Email, expiryDateTime);
                    await publicationSubscriptionRepository.UpdateSubscriber(pendingSubscriptionTable,
                        new SubscriptionEntityOld(req.Id, req.Email, req.Title, req.Slug, expiryDateTime));

                    var values = new Dictionary<string, dynamic>
                    {
                        { "publication_name", req.Title },
                        {
                            "verification_link",
                            $"{_appSettingsOptions.PublicAppUrl}/subscriptions/{req.Slug}/confirm-subscription/{activationCode}"
                        }
                    };

                    emailService.SendEmail(
                        email: req.Email,
                        templateId: _emailTemplateOptions.SubscriptionVerificationId,
                        values);

                    return new OkObjectResult(new SubscriptionStateDto
                    {
                        Slug = req.Slug,
                        Title = req.Title,
                        Status = SubscriptionStatus.SubscriptionPending
                    });
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        catch (NotifyClientException e)
        {
            logger.LogError(e, "Caught exception sending email");

            // Remove the subscriber from storage if we could not successfully send the email & just added it
            if (subscription.Status is not SubscriptionStatus.SubscriptionPending)
            {
                await publicationSubscriptionRepository.RemoveSubscriber(pendingSubscriptionTable,
                    new SubscriptionEntityOld(req.Id, req.Email));
            }

            return new BadRequestResult();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Caught exception storing subscription");
            return new BadRequestResult();
        }
    }

    [Function(FunctionNames.Unsubscribe)]
    public async Task<IActionResult> Unsubscribe(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "publication/{id}/unsubscribe/{token}")]
        FunctionContext context,
        string id,
        string token)
    {
        logger.LogInformation("{FunctionName} triggered", context.FunctionDefinition.Name);

        var email = tokenService.GetEmailFromToken(token);
        if (email is null)
        {
            return new BadRequestObjectResult("Unable to unsubscribe. A valid email address could not be parsed from the given token.");
        }

        var table = await publicationSubscriptionRepository.GetTable(NotifierTableStorage.PublicationSubscriptionsTable);
        var sub = await publicationSubscriptionRepository.RetrieveSubscriber(table, new SubscriptionEntityOld(id, email));
        if (sub is null)
        {
            return new UnprocessableEntityObjectResult("Unable to unsubscribe. Given email is not currently subscribed.");
        }

        await publicationSubscriptionRepository.RemoveSubscriber(table, sub);
        return new OkObjectResult(new SubscriptionStateDto
        {
            Slug = sub.Slug,
            Title = sub.Title,
            Status = SubscriptionStatus.NotSubscribed
        });
    }


    [Function(FunctionNames.VerifySubscription)]
    public async Task<IActionResult> VerifySubscription(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "publication/{id}/verify-subscription/{token}")]
        FunctionContext context,
        string id,
        string token)
    {
        logger.LogInformation("{FunctionName} triggered", context.FunctionDefinition.Name);

        var email = tokenService.GetEmailFromToken(token);

        if (email != null)
        {
            var subscriptionsTbl = await publicationSubscriptionRepository.GetTable(NotifierTableStorage.PublicationSubscriptionsTable);
            var pendingSubscriptionsTbl =
                await publicationSubscriptionRepository.GetTable(NotifierTableStorage.PublicationPendingSubscriptionsTable);

            var sub = publicationSubscriptionRepository
                .RetrieveSubscriber(pendingSubscriptionsTbl, new SubscriptionEntityOld(id, email)).Result;

            if (sub != null)
            {
                // Remove the pending subscription from the the file now verified
                logger.LogDebug("Removing address from pending subscribers");
                await publicationSubscriptionRepository.RemoveSubscriber(pendingSubscriptionsTbl, sub);

                // Add them to the verified subscribers table
                logger.LogDebug("Adding address to the verified subscribers");
                sub.DateTimeCreated = DateTime.UtcNow;
                await publicationSubscriptionRepository.UpdateSubscriber(subscriptionsTbl, sub);
                var unsubscribeToken =
                    tokenService.GenerateToken(sub.RowKey, DateTime.UtcNow.AddYears(1));

                var values = new Dictionary<string, dynamic>
                {
                    { "publication_name", sub.Title },
                    {
                        "unsubscribe_link",
                        $"{_appSettingsOptions.PublicAppUrl}/subscriptions/{sub.Slug}/confirm-unsubscription/{unsubscribeToken}"
                    }
                };

                emailService.SendEmail(
                    email: email,
                    templateId: _emailTemplateOptions.SubscriptionConfirmationId,
                    values);

                return new OkObjectResult(new SubscriptionStateDto
                {
                    Slug = sub.Slug,
                    Title = sub.Title,
                    Status = SubscriptionStatus.Subscribed
                });
            }
        }

        return new BadRequestObjectResult("Verification-Error");
    }

    [Function(FunctionNames.RemoveExpiredSubscriptions)]
    public async Task RemoveExpiredSubscriptions(
        [TimerTrigger("0 0 * * * *")] TimerInfo myTimer,
        FunctionContext context)
    {
        logger.LogInformation("{FunctionName} triggered", context.FunctionDefinition.Name);

        // @MarkFix check this
        // @MarkFix could move this to a repository...
        // @MarkFix could also rename ApiSubscriptionTableStorageService...
        var results = await apiSubscriptionTableStorageService.QueryEntities<SubscriptionEntity>(
            tableName: NotifierTableStorage.PublicationPendingSubscriptionsTable,
            sub => sub.DateTimeCreated < DateTime.UtcNow.AddHours(1));
        var pendingSubsToRemove = await results.ToListAsync();

        await apiSubscriptionTableStorageService.BatchManipulateEntities(
            NotifierTableStorage.PublicationPendingSubscriptionsTable,
            pendingSubsToRemove,
            TableTransactionActionType.Delete);
    }
}
