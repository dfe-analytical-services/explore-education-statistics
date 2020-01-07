using System.Collections.Generic;
using Notify.Client;
using Notify.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class EmailService : IEmailService
    {
        private readonly INotificationClient _client;

        public EmailService(INotificationClient client)
        {
            _client = client;
        }
        
        public void SendEmail(string email,
            string templateId,
            Dictionary<string, dynamic> values)
        {
            _client.SendEmail(emailAddress: email, templateId: templateId, personalisation: values);
        }
    }
}