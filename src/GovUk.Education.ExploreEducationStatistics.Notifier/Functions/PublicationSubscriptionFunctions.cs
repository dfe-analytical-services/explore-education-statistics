using FluentValidation;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Notifier.Configuration;
using GovUk.Education.ExploreEducationStatistics.Notifier.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Notify.Exceptions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Notifier.Requests;
using GovUk.Education.ExploreEducationStatistics.Notifier.Types;
using FromBodyAttribute = Microsoft.Azure.Functions.Worker.Http.FromBodyAttribute;
using static GovUk.Education.ExploreEducationStatistics.Common.TableStorageTableNames;
using GovUk.Education.ExploreEducationStatistics.Notifier.Repositories.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Notifier.Functions;

public class PublicationSubscriptionFunctions(
    ILogger<PublicationSubscriptionFunctions> logger,
    IOptions<AppSettingsOptions> appSettingsOptions,
    IOptions<GovUkNotifyOptions> govUkNotifyOptions,
    ITokenService tokenService,
    IEmailService emailService,
    IPublicationSubscriptionRepository publicationSubscriptionRepository,
    IValidator<PendingPublicationSubscriptionCreateRequest> requestValidator)
{
    private readonly AppSettingsOptions _appSettingsOptions = appSettingsOptions.Value;
    private readonly GovUkNotifyOptions.EmailTemplateOptions _emailTemplateOptions = govUkNotifyOptions.Value.EmailTemplates;
    private readonly ITokenService _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
    private readonly IEmailService _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
    private readonly IPublicationSubscriptionRepository _publicationSubscriptionRepository = publicationSubscriptionRepository ?? throw new ArgumentNullException(nameof(publicationSubscriptionRepository));

    [Function("RequestPendingSubscription")]
    // ReSharper disable once UnusedMember.Global
    public async Task<IActionResult> RequestPendingSubscriptionFunc(
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
            await _publicationSubscriptionRepository.GetTable(Constants.NotifierTableStorageTableNames.PublicationPendingSubscriptionsTableName);

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
                            _tokenService.GenerateToken(subscription.Subscriber.RowKey,
                                DateTime.UtcNow.AddYears(1));

                        var confirmationValues = new Dictionary<string, dynamic>
                        {
                            { "publication_name", subscription.Subscriber.Title },
                            {
                                "unsubscribe_link",
                                $"{_appSettingsOptions.PublicAppUrl}/subscriptions/{req.Slug}/confirm-unsubscription/{unsubscribeToken}"
                            }
                        };

                        _emailService.SendEmail(
                            email: req.Email,
                            templateId: _emailTemplateOptions.SubscriptionConfirmationId,
                            confirmationValues);

                        return new OkResult();
                    }

                case SubscriptionStatus.NotSubscribed:
                    // Verification Token expires in 1 hour
                    var expiryDateTime = DateTime.UtcNow.AddHours(1);
                    var activationCode = _tokenService.GenerateToken(req.Email, expiryDateTime);
                    await _publicationSubscriptionRepository.UpdateSubscriber(pendingSubscriptionTable,
                        new SubscriptionEntity(req.Id, req.Email, req.Title, req.Slug, expiryDateTime));

                    var values = new Dictionary<string, dynamic>
                    {
                        { "publication_name", req.Title },
                        {
                            "verification_link",
                            $"{_appSettingsOptions.PublicAppUrl}/subscriptions/{req.Slug}/confirm-subscription/{activationCode}"
                        }
                    };

                    _emailService.SendEmail(
                        email: req.Email,
                        templateId: _emailTemplateOptions.SubscriptionVerificationId,
                        values);

                    return new OkObjectResult(new SubscriptionStateDto()
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
                await _publicationSubscriptionRepository.RemoveSubscriber(pendingSubscriptionTable,
                    new SubscriptionEntity(req.Id, req.Email));
            }

            return new BadRequestResult();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Caught exception storing subscription");
            return new BadRequestResult();
        }
    }

    [Function("PublicationUnsubscribe")]
    // ReSharper disable once UnusedMember.Global
    public async Task<IActionResult> PublicationUnsubscribeFunc(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "publication/{id}/unsubscribe/{token}")]
        FunctionContext context,
        string id,
        string token)
    {
        logger.LogInformation("{FunctionName} triggered", context.FunctionDefinition.Name);

        var email = _tokenService.GetEmailFromToken(token);
        if (email is null)
        {
            return new BadRequestObjectResult("Unable to unsubscribe. A valid email address could not be parsed from the given token.");
        }

        var table = await _publicationSubscriptionRepository.GetTable(Constants.NotifierTableStorageTableNames.PublicationSubscriptionsTableName);
        var sub = await _publicationSubscriptionRepository.RetrieveSubscriber(table, new SubscriptionEntity(id, email));
        if (sub is null)
        {
            return new UnprocessableEntityObjectResult("Unable to unsubscribe. Given email is not currently subscribed.");
        }

        await _publicationSubscriptionRepository.RemoveSubscriber(table, sub);
        return new OkObjectResult(new SubscriptionStateDto()
        {
            Slug = sub.Slug,
            Title = sub.Title,
            Status = SubscriptionStatus.NotSubscribed
        });
    }


    [Function("VerifySubscription")]
    // ReSharper disable once UnusedMember.Global
    public async Task<IActionResult> VerifySubscriptionFunc(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "publication/{id}/verify-subscription/{token}")]
        FunctionContext context,
        string id,
        string token)
    {
        logger.LogInformation("{FunctionName} triggered", context.FunctionDefinition.Name);

        var email = _tokenService.GetEmailFromToken(token);

        if (email != null)
        {
            var subscriptionsTbl = await _publicationSubscriptionRepository.GetTable(Constants.NotifierTableStorageTableNames.PublicationSubscriptionsTableName);
            var pendingSubscriptionsTbl =
                await _publicationSubscriptionRepository.GetTable(Constants.NotifierTableStorageTableNames.PublicationPendingSubscriptionsTableName);

            var sub = _publicationSubscriptionRepository
                .RetrieveSubscriber(pendingSubscriptionsTbl, new SubscriptionEntity(id, email)).Result;

            if (sub != null)
            {
                // Remove the pending subscription from the the file now verified
                logger.LogDebug("Removing address from pending subscribers");
                await _publicationSubscriptionRepository.RemoveSubscriber(pendingSubscriptionsTbl, sub);

                // Add them to the verified subscribers table
                logger.LogDebug("Adding address to the verified subscribers");
                sub.DateTimeCreated = DateTime.UtcNow;
                await _publicationSubscriptionRepository.UpdateSubscriber(subscriptionsTbl, sub);
                var unsubscribeToken =
                    _tokenService.GenerateToken(sub.RowKey, DateTime.UtcNow.AddYears(1));

                var values = new Dictionary<string, dynamic>
                {
                    { "publication_name", sub.Title },
                    {
                        "unsubscribe_link",
                        $"{_appSettingsOptions.PublicAppUrl}/subscriptions/{sub.Slug}/confirm-unsubscription/{unsubscribeToken}"
                    }
                };

                _emailService.SendEmail(
                    email: email,
                    templateId: _emailTemplateOptions.SubscriptionConfirmationId,
                    values);

                return new OkObjectResult(new SubscriptionStateDto()
                {
                    Slug = sub.Slug,
                    Title = sub.Title,
                    Status = SubscriptionStatus.Subscribed
                });
            }
        }

        return new BadRequestObjectResult("Verification-Error");
    }

    [Function("RemoveNonVerifiedSubscriptions")]
    // ReSharper disable once UnusedMember.Global
    public async Task RemoveNonVerifiedSubscriptionsFunc(
        [TimerTrigger("0 0 * * * *")] TimerInfo myTimer,
        FunctionContext context)
    {
        logger.LogInformation("{FunctionName} triggered", context.FunctionDefinition.Name);

        var pendingSubscriptionsTbl = await _publicationSubscriptionRepository.GetTable(Constants.NotifierTableStorageTableNames.PublicationPendingSubscriptionsTableName);

        // Remove any pending subscriptions where the token has expired i.e. more than 1 hour old
        var query = new TableQuery<SubscriptionEntity>()
            .Where(TableQuery.GenerateFilterConditionForDate("DateTimeCreated", QueryComparisons.LessThan,
                DateTime.UtcNow.AddHours(1)));

        TableContinuationToken? token = null;
        do
        {
            var resultSegment = await pendingSubscriptionsTbl.ExecuteQuerySegmentedAsync(query, token);
            token = resultSegment.ContinuationToken;

            foreach (var entity in resultSegment.Results)
            {
                await _publicationSubscriptionRepository.RemoveSubscriber(pendingSubscriptionsTbl, entity);
            }
        } while (token != null);
    }
}
