using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Notifier.Options;
using GovUk.Education.ExploreEducationStatistics.Notifier.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Notify.Exceptions;
using Notify.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Notifier.Services;

public class EmailService(
    INotificationClient notificationClient,
    IOptions<AppOptions> appOptions,
    ILogger<EmailService> logger) : IEmailService
{
    public void SendEmail(
        string email,
        string templateId,
        Dictionary<string, dynamic> values)
    {
        try
        {
            notificationClient.SendEmail(
                emailAddress: email,
                templateId: templateId,
                personalisation: values,
                clientReference: null,
                emailReplyToId: null,
                oneClickUnsubscribeURL: null);
        }
        catch (NotifyClientException e)
        {
            // In non-Production environments, we use team-only API keys which are limited
            // to sending emails only to people in our team within GovUK Notify.
            //
            // On non-Production environments, we have test users who don't form a part of
            // that team and so for each of them being targeted as recipients of emails,
            // we will receive an error detailing that an email cannot be sent to them.
            //
            // In these instances we will want to catch and log these errors rather than
            // have them appear in Application Insights as alerts.
            if (appOptions.Value.SuppressExceptionsForTeamOnlyApiKeyErrors
                && e.Message.Contains("team-only API key"))
            {
                logger.LogInformation(
                    "Email could not be sent to \"{Email}\" as they are not a valid " +
                    "recipient for this team-only API key.",
                    email);
            }
            else
            {
                throw;
            }
        }
    }
}
