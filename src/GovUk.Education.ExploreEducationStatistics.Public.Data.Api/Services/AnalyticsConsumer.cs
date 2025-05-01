using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services;

public class AnalyticsConsumer(
    IAnalyticsManager manager,
    IAnalyticsWriter analyticsWriter,
    ILogger<AnalyticsConsumer> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)  
        {
            try
            {
                var message = await manager.Read(stoppingToken);
                await analyticsWriter.Report(message, stoppingToken);
            }
            catch (Exception e)
            {
                logger.LogError(
                    exception: e,
                    message: "Failed to read/report request recorded for analytics");
            }
        }       
    }
}
