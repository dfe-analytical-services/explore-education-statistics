#nullable enable
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Notify.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Notifier.Services;

public class EmailService : IEmailService
{
    private readonly INotificationClient _client;
    private readonly ILogger<IEmailService> _logger;

    public EmailService(INotificationClient client,
        ILogger<IEmailService> logger)
    {
        _client = client;
        _logger = logger;
    }

    public void SendEmail(string email,
        string templateId,
        Dictionary<string, dynamic> values)
    {
        _client.SendEmail(email, templateId, values);
    }
}
