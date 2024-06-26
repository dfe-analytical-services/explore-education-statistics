using System.Collections.Generic;
using Notify.Client;

namespace GovUk.Education.ExploreEducationStatistics.Notifier.Services.Interfaces;

public interface IEmailService
{
    void SendEmail(NotificationClient client, string email, string templateId, Dictionary<string, dynamic> values);
}
