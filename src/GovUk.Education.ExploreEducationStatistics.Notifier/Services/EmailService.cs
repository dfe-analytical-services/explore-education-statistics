using System.Collections.Generic;
using Notify.Client;
using Notify.Models.Responses;

namespace GovUk.Education.ExploreEducationStatistics.Notifier.Services
{
    public class EmailService : IEmailService
    {
        public void sendEmail(NotificationClient client, string email, string templateId, Dictionary<string, dynamic> values)
        {
            EmailNotificationResponse response = client.SendEmail(
                emailAddress: email,
                templateId: templateId,
                personalisation: values
            );
        }
    }
}