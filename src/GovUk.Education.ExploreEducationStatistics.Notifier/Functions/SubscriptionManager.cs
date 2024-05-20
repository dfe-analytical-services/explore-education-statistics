using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Notifier.Configuration;
using GovUk.Education.ExploreEducationStatistics.Notifier.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Notify.Exceptions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Notifier.Types;
using static GovUk.Education.ExploreEducationStatistics.Common.TableStorageTableNames;

namespace GovUk.Education.ExploreEducationStatistics.Notifier.Functions
{
    public class SubscriptionManager(ILogger<SubscriptionManager> logger,
        IOptions<AppSettingOptions> appSettingOptions,
        IOptions<GovUkNotifyOptions> govUkNotifyOptions,
        ITokenService tokenService,
        IEmailService emailService,
        IStorageTableService storageTableService,
        INotificationClientProvider notificationClientProvider)
    {
        private readonly AppSettingOptions _appSettingOptions = appSettingOptions.Value;
        private readonly GovUkNotifyOptions.EmailTemplateOptions _emailTemplateOptions = govUkNotifyOptions.Value.EmailTemplates;
        private readonly ITokenService _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        private readonly IEmailService _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        private readonly IStorageTableService _storageTableService = storageTableService ?? throw new ArgumentNullException(nameof(storageTableService));
        private readonly INotificationClientProvider _notificationClientProvider = notificationClientProvider ??
                                                                                   throw new ArgumentNullException(nameof(notificationClientProvider));

        [Function("RequestPendingSubscription")]
        // ReSharper disable once UnusedMember.Global
        public async Task<IActionResult> RequestPendingSubscriptionFunc(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "publication/request-pending-subscription/")]
            HttpRequest req,
            FunctionContext context)
        {
            logger.LogInformation("{FunctionName} triggered", context.FunctionDefinition.Name);

            string? id = req.Query["id"];
            string? email = req.Query["email"];
            string? slug = req.Query["slug"];
            string? title = req.Query["title"];
            var data = await req.GetJsonBody();
            id ??= data?.id;
            email ??= data?.email;
            slug ??= data?.slug;
            title ??= data?.title;

            if (id == null || email == null || slug == null || title == null)
            {
                return new BadRequestObjectResult("Please pass a valid email & publication");
            }

            var notificationClient = _notificationClientProvider.Get();

            var subscription = await storageTableService.GetSubscription(id, email);
            var pendingSubscriptionTable =
                await _storageTableService.GetTable(NotifierPendingSubscriptionsTableName);

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
                                {
                                    "publication_name", subscription.Subscriber.Title
                                },
                                {
                                    "unsubscribe_link",
                                    $"{_appSettingOptions.PublicAppUrl}/subscriptions/{slug}/confirm-unsubscription/{unsubscribeToken}"
                                }
                            };

                            _emailService.SendEmail(notificationClient,
                                email: email,
                                templateId: _emailTemplateOptions.SubscriptionConfirmationId,
                                confirmationValues);

                            return new OkResult();
                        }

                    case SubscriptionStatus.NotSubscribed:
                        // Verification Token expires in 1 hour
                        var expiryDateTime = DateTime.UtcNow.AddHours(1);
                        var activationCode = _tokenService.GenerateToken(email, expiryDateTime);
                        await _storageTableService.UpdateSubscriber(pendingSubscriptionTable,
                            new SubscriptionEntity(id, email, title, slug, expiryDateTime));

                        var values = new Dictionary<string, dynamic>
                        {
                            {
                                "publication_name", title
                            },
                            {
                                "verification_link",
                                $"{_appSettingOptions.PublicAppUrl}/subscriptions/{slug}/confirm-subscription/{activationCode}"
                            }
                        };

                        _emailService.SendEmail(notificationClient,
                            email: email,
                            templateId: _emailTemplateOptions.SubscriptionVerificationId,
                            values);

                        return new OkObjectResult(new SubscriptionStateDto()
                        {
                            Slug = slug,
                            Title = title,
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
                    await _storageTableService.RemoveSubscriber(pendingSubscriptionTable,
                        new SubscriptionEntity(id, email));
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
            HttpRequest req,
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

            var table = await _storageTableService.GetTable(NotifierSubscriptionsTableName);
            var sub = _storageTableService.RetrieveSubscriber(table, new SubscriptionEntity(id, email)).Result;
            if (sub is null)
            {
                return new UnprocessableEntityObjectResult("Unable to unsubscribe. Given email is not currently subscribed.");
            }

            await _storageTableService.RemoveSubscriber(table, sub);
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
            HttpRequest req,
            FunctionContext context,
            string id,
            string token)
        {
            logger.LogInformation("{FunctionName} triggered", context.FunctionDefinition.Name);

            var email = _tokenService.GetEmailFromToken(token);

            var notificationClient = _notificationClientProvider.Get();

            if (email != null)
            {
                var subscriptionsTbl = await _storageTableService.GetTable(NotifierSubscriptionsTableName);
                var pendingSubscriptionsTbl =
                    await _storageTableService.GetTable(NotifierPendingSubscriptionsTableName);

                var sub = _storageTableService
                    .RetrieveSubscriber(pendingSubscriptionsTbl, new SubscriptionEntity(id, email)).Result;

                if (sub != null)
                {
                    // Remove the pending subscription from the the file now verified
                    logger.LogDebug("Removing address from pending subscribers");
                    await _storageTableService.RemoveSubscriber(pendingSubscriptionsTbl, sub);

                    // Add them to the verified subscribers table
                    logger.LogDebug("Adding address to the verified subscribers");
                    sub.DateTimeCreated = DateTime.UtcNow;
                    await _storageTableService.UpdateSubscriber(subscriptionsTbl, sub);
                    var unsubscribeToken =
                        _tokenService.GenerateToken(sub.RowKey, DateTime.UtcNow.AddYears(1));

                    var values = new Dictionary<string, dynamic>
                    {
                        {
                            "publication_name", sub.Title
                        },
                        {
                            "unsubscribe_link",
                            $"{_appSettingOptions.PublicAppUrl}/subscriptions/{sub.Slug}/confirm-unsubscription/{unsubscribeToken}"
                        }
                    };

                    _emailService.SendEmail(notificationClient,
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

            var pendingSubscriptionsTbl = await _storageTableService.GetTable(NotifierPendingSubscriptionsTableName);

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
                    await _storageTableService.RemoveSubscriber(pendingSubscriptionsTbl, entity);
                }
            } while (token != null);
        }
    }
}
