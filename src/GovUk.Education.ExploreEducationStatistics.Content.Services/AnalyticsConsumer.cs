using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using Microsoft.Extensions.Hosting;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services;

public class AnalyticsConsumer(
    IAnalyticsManager analyticsManager,
    IAnalyticsWriter analyticsWriter) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var request = await Task.WhenAny(analyticsManager.ReadZipAsync(), analyticsManager.ReadCsvAsync());

            // switch

            var zipDownloadRequest = analyticsManager.TryReadZipDownload();
            if (zipDownloadRequest is not null)
            {
                await analyticsWriter.ReportZipDownload(zipDownloadRequest);
            }

            var csvDownloadRequest = analyticsManager.TryReadCsvDownload();
            if (csvDownloadRequest is not null)
            {
                await analyticsWriter.ReportCsvDownload(csvDownloadRequest);
            }
        }
    }
}
