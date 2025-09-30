using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.Data.Tables;
using FluentValidation;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Notifier.Model;
using GovUk.Education.ExploreEducationStatistics.Notifier.Options;
using GovUk.Education.ExploreEducationStatistics.Notifier.Repositories.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Notifier.Requests;
using GovUk.Education.ExploreEducationStatistics.Notifier.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Notifier.Types;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Notify.Exceptions;
using FromBodyAttribute = Microsoft.Azure.Functions.Worker.Http.FromBodyAttribute;

namespace GovUk.Education.ExploreEducationStatistics.Notifier.Functions;

public class PublicationSubscriptionFunctions(
    ContentDbContext contentDbContext,
    ILogger<PublicationSubscriptionFunctions> logger,
    IOptions<AppOptions> appOptions,
    IOptions<GovUkNotifyOptions> govUkNotifyOptions,
    ITokenService tokenService,
    IEmailService emailService,
    IValidator<PendingPublicationSubscriptionCreateRequest> requestValidator,
    INotifierTableStorageService notifierTableStorageService,
    ISubscriptionRepository subscriptionRepository
)
{
    private readonly AppOptions _appOptions = appOptions.Value;
    private readonly GovUkNotifyOptions.EmailTemplateOptions _emailTemplateOptions =
        govUkNotifyOptions.Value.EmailTemplates;

    private static class FunctionNames
    {
        private const string Base = "PublicationSubscriptions_";
        public const string RequestPendingSubscription =
            $"{Base}{nameof(PublicationSubscriptionFunctions.RequestPendingSubscription)}";
        public const string RemoveExpiredSubscriptions =
            $"{Base}{nameof(PublicationSubscriptionFunctions.RemoveExpiredSubscriptions)}";
        public const string Unsubscribe =
            $"{Base}{nameof(PublicationSubscriptionFunctions.Unsubscribe)}";
        public const string VerifySubscription =
            $"{Base}{nameof(PublicationSubscriptionFunctions.VerifySubscription)}";
    }

    [Function(FunctionNames.RequestPendingSubscription)]
    public async Task<IActionResult> RequestPendingSubscription(
        [HttpTrigger(
            AuthorizationLevel.Anonymous,
            "post",
            Route = "publication/request-pending-subscription/"
        )]
        [FromBody]
            PendingPublicationSubscriptionCreateRequest req,
        FunctionContext context,
        CancellationToken cancellationToken
    )
    {
        logger.LogInformation("{FunctionName} triggered", context.FunctionDefinition.Name);

        var validationResult = await requestValidator.Validate(req, cancellationToken);
        if (validationResult.IsLeft) // If validation failed
        {
            logger.LogError(
                "{FunctionName} failed because the {req} did not contain a valid JSON object.",
                context.FunctionDefinition.Name,
                req
            );
            return validationResult.Left;
        }

        var subscription = await subscriptionRepository.GetSubscriptionAndStatus(req.Id, req.Email);

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
                    var unsubscribeToken = tokenService.GenerateToken(
                        subscription.Entity!.RowKey,
                        DateTime.UtcNow.AddYears(1)
                    );

                    var confirmationValues = new Dictionary<string, dynamic>
                    {
                        { "publication_name", subscription.Entity.Title },
                        {
                            "unsubscribe_link",
                            $"{_appOptions.PublicAppUrl}/subscriptions/{req.Slug}/confirm-unsubscription/{unsubscribeToken}"
                        },
                    };

                    emailService.SendEmail(
                        email: req.Email,
                        templateId: _emailTemplateOptions.SubscriptionConfirmationId,
                        confirmationValues
                    );

                    return new OkResult();
                }

                // Adding new pending sub if no pending or active subscription found
                case SubscriptionStatus.NotSubscribed:
                    // Verification Token expires in 1 hour
                    var expiryDateTime = DateTime.UtcNow.AddHours(1);
                    var activationCode = tokenService.GenerateToken(req.Email, expiryDateTime);

                    await notifierTableStorageService.CreateEntity(
                        NotifierTableStorage.PublicationPendingSubscriptionsTable,
                        new SubscriptionEntity
                        {
                            PartitionKey = req.Id,
                            RowKey = req.Email,
                            Slug = req.Slug,
                            Title = req.Title,
                            DateTimeCreated = expiryDateTime, // DateTimeCreated is a misleading name!
                        },
                        cancellationToken
                    );

                    var values = new Dictionary<string, dynamic>
                    {
                        { "publication_name", req.Title },
                        {
                            "verification_link",
                            $"{_appOptions.PublicAppUrl}/subscriptions/{req.Slug}/confirm-subscription/{activationCode}"
                        },
                    };

                    emailService.SendEmail(
                        email: req.Email,
                        templateId: _emailTemplateOptions.SubscriptionVerificationId,
                        values
                    );

                    return new OkObjectResult(
                        new SubscriptionStateDto
                        {
                            Slug = req.Slug,
                            Title = req.Title,
                            Status = SubscriptionStatus.SubscriptionPending,
                        }
                    );
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        catch (NotifyClientException e)
        {
            logger.LogError(e, "Caught exception sending email");

            // If we fail to send email, clean up entity in pending subs table that we just created
            if (subscription.Status is not SubscriptionStatus.SubscriptionPending)
            {
                await notifierTableStorageService.DeleteEntity(
                    tableName: NotifierTableStorage.PublicationPendingSubscriptionsTable,
                    partitionKey: req.Id,
                    rowKey: req.Email,
                    cancellationToken: cancellationToken
                );
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
        [HttpTrigger(
            AuthorizationLevel.Anonymous,
            "get",
            Route = "publication/{publicationId}/unsubscribe/{token}"
        )]
            FunctionContext context,
        string publicationId,
        string token
    )
    {
        logger.LogInformation("{FunctionName} triggered", context.FunctionDefinition.Name);

        var email = tokenService.GetEmailFromToken(token);
        if (email is null)
        {
            return new BadRequestObjectResult(
                "Unable to unsubscribe. A valid email address could not be parsed from the given token."
            );
        }

        var subscription = await notifierTableStorageService.GetEntityIfExists<SubscriptionEntity>(
            tableName: NotifierTableStorage.PublicationSubscriptionsTable,
            partitionKey: publicationId,
            rowKey: email
        );

        if (subscription is null)
        {
            return new UnprocessableEntityObjectResult(
                "Unable to unsubscribe. Given email is not currently subscribed."
            );
        }

        await notifierTableStorageService.DeleteEntity(
            tableName: NotifierTableStorage.PublicationSubscriptionsTable,
            partitionKey: publicationId,
            rowKey: email
        );

        var supersededPublicationIds = await contentDbContext
            .Publications.Where(p => p.SupersededById == Guid.Parse(publicationId))
            .Select(p => p.Id)
            .ToListAsync();

        foreach (var supersededPubId in supersededPublicationIds)
        {
            await notifierTableStorageService.DeleteEntity(
                tableName: NotifierTableStorage.PublicationSubscriptionsTable,
                partitionKey: supersededPubId.ToString(),
                rowKey: email
            );
        }

        return new OkObjectResult(
            new SubscriptionStateDto
            {
                Slug = subscription.Slug,
                Title = subscription.Title,
                Status = SubscriptionStatus.NotSubscribed,
            }
        );
    }

    [Function(FunctionNames.VerifySubscription)]
    public async Task<IActionResult> VerifySubscription(
        [HttpTrigger(
            AuthorizationLevel.Anonymous,
            "get",
            Route = "publication/{publicationId}/verify-subscription/{token}"
        )]
            FunctionContext context,
        Guid publicationId,
        string token
    )
    {
        logger.LogInformation("{FunctionName} triggered", context.FunctionDefinition.Name);

        var email = tokenService.GetEmailFromToken(token);

        if (email == null)
        {
            return new BadRequestObjectResult(
                "Unable to unsubscribe. A valid email address could not be parsed from the given token."
            );
        }

        var pendingSubscription =
            await notifierTableStorageService.GetEntityIfExists<SubscriptionEntity>(
                tableName: NotifierTableStorage.PublicationPendingSubscriptionsTable,
                partitionKey: publicationId.ToString(),
                rowKey: email
            );

        if (pendingSubscription == null)
        {
            return new NotFoundObjectResult("Subscription for email derived from token not found");
        }

        logger.LogDebug("Removing address from pending subscribers");

        await notifierTableStorageService.DeleteEntity(
            tableName: NotifierTableStorage.PublicationPendingSubscriptionsTable,
            partitionKey: pendingSubscription.PublicationId,
            rowKey: pendingSubscription.Email
        );

        logger.LogDebug("Adding address to the verified subscribers");

        var newSubscription = new SubscriptionEntity
        {
            PartitionKey = pendingSubscription.PublicationId,
            RowKey = pendingSubscription.Email,
            Slug = pendingSubscription.Slug,
            Title = pendingSubscription.Title,
            DateTimeCreated = DateTime.UtcNow,
        };

        await notifierTableStorageService.CreateEntity(
            tableName: NotifierTableStorage.PublicationSubscriptionsTable,
            entity: newSubscription
        );

        var unsubscribeToken = tokenService.GenerateToken(
            newSubscription.Email,
            DateTime.UtcNow.AddYears(1)
        );

        var values = new Dictionary<string, dynamic>
        {
            { "publication_name", newSubscription.Title },
            {
                "unsubscribe_link",
                $"{_appOptions.PublicAppUrl}/subscriptions/{newSubscription.Slug}/confirm-unsubscription/{unsubscribeToken}"
            },
        };

        emailService.SendEmail(
            email: email,
            templateId: _emailTemplateOptions.SubscriptionConfirmationId,
            values
        );

        return new OkObjectResult(
            new SubscriptionStateDto
            {
                Slug = newSubscription.Slug,
                Title = newSubscription.Title,
                Status = SubscriptionStatus.Subscribed,
            }
        );
    }

    [Function(FunctionNames.RemoveExpiredSubscriptions)]
    public async Task RemoveExpiredSubscriptions(
        [TimerTrigger("0 0 * * * *")] TimerInfo myTimer,
        FunctionContext context
    )
    {
        logger.LogInformation("{FunctionName} triggered", context.FunctionDefinition.Name);

        var asyncPageable = await notifierTableStorageService.QueryEntities<SubscriptionEntity>(
            tableName: NotifierTableStorage.PublicationPendingSubscriptionsTable,
            sub => sub.DateTimeCreated < DateTime.UtcNow.AddHours(1)
        ); // WARN: DateTimeCreated is actually ExpiryTime!!!
        var pendingSubsToRemove = await asyncPageable.ToListAsync();

        await notifierTableStorageService.BatchManipulateEntities(
            NotifierTableStorage.PublicationPendingSubscriptionsTable,
            pendingSubsToRemove,
            TableTransactionActionType.Delete
        );
    }
}
