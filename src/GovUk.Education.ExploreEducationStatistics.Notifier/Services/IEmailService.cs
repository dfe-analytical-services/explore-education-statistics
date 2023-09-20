#nullable enable
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Notifier.Services;

public interface IEmailService
{
    void SendEmail(string email, string templateId, Dictionary<string, dynamic> values);
}
