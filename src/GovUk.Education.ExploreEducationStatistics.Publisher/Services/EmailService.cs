using GovUk.Education.ExploreEducationStatistics.Publisher.Options;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Notify.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services;

public class EmailService(
    INotificationClient notificationClient,
    ILogger<IEmailService> logger,
    IOptions<NotifyOptions> notifyOptions
) : IEmailService
{
    private readonly NotifyOptions.EmailTemplateOptions _emailTemplateOptions = notifyOptions.Value.EmailTemplates;

    public void NotifyEinTilesRequireUpdate(string bauEmail, string bulletsStr)
    {
        var values = new Dictionary<string, dynamic> { { "update_bullet_list", bulletsStr } };

        SendEmail(email: bauEmail, templateId: _emailTemplateOptions.EinTilesRequireUpdateId, values);
    }

    private void SendEmail(string email, string templateId, Dictionary<string, dynamic> values)
    {
        try
        {
            notificationClient.SendEmail(emailAddress: email, templateId: templateId, personalisation: values);
            // TODO EES-2752 This returns an EmailNotificationResponse containing a message id which we could store
            // if we decide to retrieve and display the delivery status of emails
        }
        catch (Exception e)
        {
            logger.LogError(e, "Exception occured while sending email");
        }
    }
}
