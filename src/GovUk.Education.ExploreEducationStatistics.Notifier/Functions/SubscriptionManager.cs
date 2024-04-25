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
using static GovUk.Education.ExploreEducationStatistics.Common.TableStorageTableNames;

namespace GovUk.Education.ExploreEducationStatistics.Notifier.Functions
{
    public class SubscriptionManager
    {
        private readonly ILogger<SubscriptionManager> _logger;
        private readonly AppSettingOptions _appSettingOptions;
        private readonly GovUkNotifyOptions.EmailTemplateOptions _emailTemplateOptions;
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;
        private readonly IStorageTableService _storageTableService;
        private readonly INotificationClientProvider _notificationClientProvider;

        public SubscriptionManager(
            ILogger<SubscriptionManager> logger,
            IOptions<AppSettingOptions> appSettingOptions,
            IOptions<GovUkNotifyOptions> govUkNotifyOptions,
            ITokenService tokenService,
            IEmailService emailService,
            IStorageTableService storageTableService,
            INotificationClientProvider notificationClientProvider)
        {
            _logger = logger;
            _appSettingOptions = appSettingOptions.Value;
            _emailTemplateOptions = govUkNotifyOptions.Value.EmailTemplates;
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _storageTableService = storageTableService ?? throw new ArgumentNullException(nameof(storageTableService));
            _notificationClientProvider = notificationClientProvider ??
                                          throw new ArgumentNullException(nameof(notificationClientProvider));
        }

        [Function("PublicationSubscribe")]
        // ReSharper disable once UnusedMember.Global
        public async Task<IActionResult> PublicationSubscribeFunc(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "publication/subscribe/")]
            HttpRequest req,
            FunctionContext context)
        {
            _logger.LogInformation("{FunctionName} triggered", context.FunctionDefinition.Name);

            var subscriptionsTable = await _storageTableService.GetTable(NotifierSubscriptionsTableName);
            var pendingSubscriptionsTable = await _storageTableService.GetTable(NotifierPendingSubscriptionsTableName);

            var notificationClient = _notificationClientProvider.Get();

            string? id = req.Query["id"];
            string? email = req.Query["email"];
            string? slug = req.Query["slug"];
            string? title = req.Query["title"];

            var subscriptionPending = false;
            var data = await req.GetJsonBody();

            id ??= data?.id;
            email ??= data?.email;
            slug ??= data?.slug;
            title ??= data?.title;

            if (id == null || email == null || slug == null || title == null)
            {
                return new BadRequestObjectResult("Please pass a valid email & publication");
            }

            try
            {
                var pendingSub =
                    await _storageTableService.RetrieveSubscriber(pendingSubscriptionsTable,
                        new SubscriptionEntity(id, email));
                subscriptionPending = pendingSub != null;

                _logger.LogDebug("Pending subscription found?: {PendingSubscription}", subscriptionPending);

                // If already existing and pending then don't send another one
                if (!subscriptionPending)
                {
                    // Now check if already subscribed

                    var activeSubscriber = await _storageTableService
                        .RetrieveSubscriber(subscriptionsTable, new SubscriptionEntity(id, email));
                    if (activeSubscriber != null)
                    {
                        var unsubscribeToken =
                            _tokenService.GenerateToken(activeSubscriber.RowKey,
                                DateTime.UtcNow.AddYears(1));

                        var confirmationValues = new Dictionary<string, dynamic>
                        {
                            {
                                "publication_name", activeSubscriber.Title
                            },
                            {
                                "unsubscribe_link",
                                $"{_appSettingOptions.BaseUrl}/publication/{activeSubscriber.PartitionKey}/unsubscribe/{unsubscribeToken}"
                            }
                        };

                        _emailService.SendEmail(notificationClient,
                            email: email,
                            templateId: _emailTemplateOptions.SubscriptionConfirmationId,
                            confirmationValues);

                        return new OkResult();
                    }

                    // Verification Token expires in 1 hour
                    var expiryDateTime = DateTime.UtcNow.AddHours(1);
                    var activationCode = _tokenService.GenerateToken(email, expiryDateTime);

                    await _storageTableService.UpdateSubscriber(pendingSubscriptionsTable,
                        new SubscriptionEntity(id, email, title, slug, expiryDateTime));

                    var values = new Dictionary<string, dynamic>
                    {
                        {
                            "publication_name", title
                        },
                        {
                            "verification_link",
                            $"{_appSettingOptions.BaseUrl}/publication/{id}/verify-subscription/{activationCode}"
                        }
                    };

                    _emailService.SendEmail(notificationClient,
                        email: email,
                        templateId: _emailTemplateOptions.SubscriptionVerificationId,
                        values);
                }

                return new OkResult();
            }
            catch (NotifyClientException e)
            {
                _logger.LogError(e, "Caught exception sending email");

                // Remove the subscriber from storage if we could not successfully send the email & just added it
                if (!subscriptionPending)
                {
                    await _storageTableService.RemoveSubscriber(pendingSubscriptionsTable,
                        new SubscriptionEntity(id, email));
                }

                return new BadRequestResult();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Caught exception storing subscription");
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
            _logger.LogInformation("{FunctionName} triggered", context.FunctionDefinition.Name);

            var email = _tokenService.GetEmailFromToken(token);

            if (email != null)
            {
                var table = await _storageTableService.GetTable(NotifierSubscriptionsTableName);

                var sub = new SubscriptionEntity(id, email);
                sub = _storageTableService.RetrieveSubscriber(table, sub).Result;

                if (sub != null)
                {
                    await _storageTableService.RemoveSubscriber(table, sub);
                    return new RedirectResult(
                        $"{_appSettingOptions.PublicAppUrl}/subscriptions?slug={sub.Slug}&unsubscribed=true",
                        true);
                }
            }

            return new BadRequestObjectResult("Unable to unsubscribe");
        }

        [Function("PublicationSubscriptionVerify")]
        // ReSharper disable once UnusedMember.Global
        public async Task<IActionResult> PublicationSubscriptionVerifyFunc(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "publication/{id}/verify-subscription/{token}")]
            HttpRequest req,
            FunctionContext context,
            string id,
            string token)
        {
            _logger.LogInformation("{FunctionName} triggered", context.FunctionDefinition.Name);

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
                    _logger.LogDebug("Removing address from pending subscribers");
                    await _storageTableService.RemoveSubscriber(pendingSubscriptionsTbl, sub);

                    // Add them to the verified subscribers table
                    _logger.LogDebug("Adding address to the verified subscribers");
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
                            $"{_appSettingOptions.BaseUrl}/publication/{sub.PartitionKey}/unsubscribe/{unsubscribeToken}"
                        }
                    };

                    _emailService.SendEmail(notificationClient,
                        email: email,
                        templateId: _emailTemplateOptions.SubscriptionConfirmationId,
                        values);

                    return new RedirectResult(
                        $"{_appSettingOptions.PublicAppUrl}/subscriptions?slug={sub.Slug}&verified=true",
                        true);
                }
            }

            return new RedirectResult(
                $"{_appSettingOptions.PublicAppUrl}/subscriptions/verification-error",
                true);
        }

        [Function("RemoveNonVerifiedSubscriptions")]
        // ReSharper disable once UnusedMember.Global
        public async Task RemoveNonVerifiedSubscriptionsFunc(
            [TimerTrigger("0 0 * * * *")] TimerInfo myTimer,
            FunctionContext context)
        {
            _logger.LogInformation("{FunctionName} triggered", context.FunctionDefinition.Name);

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
