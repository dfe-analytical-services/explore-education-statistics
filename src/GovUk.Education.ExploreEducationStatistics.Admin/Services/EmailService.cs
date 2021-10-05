#nullable enable
using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;
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

        public Either<ActionResult, Unit> SendEmail(
            string email,
            string templateId,
            Dictionary<string, dynamic> values)
        {
            try
            {
                _client.SendEmail(emailAddress: email, templateId: templateId, personalisation: values);
                // TODO EES-2752 This returns an EmailNotificationResponse containing a message id which we could store
                // if we decide to retrieve and display the delivery status of emails
                return Unit.Instance;
            }
            catch (Exception)
            {
                // TODO EES-2462 Handle this error
                return new NotFoundResult();
            }
        }
    }
}
