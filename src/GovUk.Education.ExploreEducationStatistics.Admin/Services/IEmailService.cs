using System.Collections.Generic;
using Notify.Client;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public interface IEmailService
    {
        void SendEmail(string email, string templateId, Dictionary<string, dynamic> values);
    }
}