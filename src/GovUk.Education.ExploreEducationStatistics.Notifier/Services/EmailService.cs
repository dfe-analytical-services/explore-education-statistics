using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Notifier.Services.Interfaces;
using Notify.Client;

namespace GovUk.Education.ExploreEducationStatistics.Notifier.Services
{
    public class EmailService : IEmailService
    {
        public void SendEmail(NotificationClient client,
            string email,
            string templateId,
            Dictionary<string, dynamic> values)
        {
            client.SendEmail(email, templateId, values);
        }
    }
}
