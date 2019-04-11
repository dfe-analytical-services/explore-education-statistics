using System.Collections.Generic;
using Notify.Client;

namespace GovUk.Education.ExploreEducationStatistics.Notifier.Services
{
    public interface IEmailService
    {
        void sendEmail(NotificationClient client, string email, string templateId, Dictionary<string, dynamic> values);
    }
}