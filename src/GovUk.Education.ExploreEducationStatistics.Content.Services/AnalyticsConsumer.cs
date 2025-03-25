using System;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services;

public class AnalyticsConsumer(
    IAnalyticsManager analyticsManager,
    IAnalyticsWriter analyticsWriter,
    ILogger<AnalyticsConsumer> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var request =
                    await analyticsManager.ReadReleaseVersionZipDownload(stoppingToken);
                await analyticsWriter.ReportReleaseVersionZipDownload(request);
            }
            catch (Exception e)
            {
                logger.LogError(
                    exception: e,
                    message: "Error whilst reading a ReleaseVersionZipDownload event from {AnalyticsManager}",
                    nameof(IAnalyticsManager));
            }
        }
    }
}
