using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Notifier.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Notifier.Services;

public class LoggingEmailService(ILogger<IEmailService> logger) : IEmailService
{
    public void SendEmail(string email, string templateId, Dictionary<string, dynamic> values)
    {
        logger.LogInformation(
            """
            Sending email to a recipient
            with template Id "{TemplateId}" and
            the following template values: 
            {TemplateValues}
            """,
            templateId,
            values
        );
    }
}
