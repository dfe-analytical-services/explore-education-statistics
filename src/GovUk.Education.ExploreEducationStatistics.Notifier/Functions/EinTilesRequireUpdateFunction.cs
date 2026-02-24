using System.Text;
using GovUk.Education.ExploreEducationStatistics.Notifier.Model;
using GovUk.Education.ExploreEducationStatistics.Notifier.Options;
using GovUk.Education.ExploreEducationStatistics.Notifier.Services.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GovUk.Education.ExploreEducationStatistics.Notifier.Functions;

public class EinTilesRequireUpdateFunction(
    IOptions<AppOptions> appOptions,
    IOptions<GovUkNotifyOptions> govUkNotifyOptions,
    IEmailService emailService,
    ILogger<EinTilesRequireUpdateFunction> logger
)
{
    private readonly AppOptions _appOptions = appOptions.Value;
    private readonly GovUkNotifyOptions.EmailTemplateOptions _emailTemplateOptions = govUkNotifyOptions
        .Value
        .EmailTemplates;

    [Function(nameof(SendEinTilesNeedUpdateEmail))]
    public void SendEinTilesNeedUpdateEmail(
        [QueueTrigger(NotifierQueueStorage.EinTilesRequireUpdate)] EinTilesRequireUpdateMessage message,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var bulletsStr = new StringBuilder();
            foreach (var page in message.Pages)
            {
                bulletsStr.Append(
                    $"* {page.Title}, {_appOptions.AdminAppUrl}/education-in-numbers/{page.Id}/content\n"
                );
                foreach (var tile in page.Tiles)
                {
                    // sub-bullets for each tile associated with a particular page
                    bulletsStr.Append(
                        $"  * Tile titled '{tile.Title}' in section '{tile.ContentSectionTitle}', which uses this data set: {_appOptions.PublicAppUrl}/data-catalogue/data-set/{tile.DataSetFileId}\n"
                    );
                }
            }
            var values = new Dictionary<string, dynamic> { { "update_bullet_list", bulletsStr.ToString() } };

            emailService.SendEmail(
                email: _appOptions.BauEmail,
                templateId: _emailTemplateOptions.EinTilesRequireUpdateId,
                values
            );
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Exception occured while executing '{FunctionName}'",
                nameof(SendEinTilesNeedUpdateEmail)
            );
        }
    }
}
