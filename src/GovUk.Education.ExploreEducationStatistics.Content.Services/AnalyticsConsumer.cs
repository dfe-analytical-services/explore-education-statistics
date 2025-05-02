#nullable enable
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
                var request = await analyticsManager.Read(stoppingToken);
                await analyticsWriter.Report(request, stoppingToken);
            }
            catch (TaskCanceledException e)
            {
                logger.LogInformation(e, "AnalyticsConsumer background task cancelled");
            }
            catch (Exception e)
            {
                logger.LogWarning(e, "Failed to read/report request recorded for analytics");
            }
        }
    }
}
