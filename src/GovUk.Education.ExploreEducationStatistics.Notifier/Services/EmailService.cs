using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Notifier.Services.Interfaces;
using Notify.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Notifier.Services;

public class EmailService(INotificationClient notificationClient) : IEmailService
{
    public void SendEmail(
        string email,
        string templateId,
        Dictionary<string, dynamic> values)
    {
        notificationClient.SendEmail(
            emailAddress: email,
            templateId: templateId, 
            personalisation: values,
            clientReference: null,
            emailReplyToId: null);
    }
}
