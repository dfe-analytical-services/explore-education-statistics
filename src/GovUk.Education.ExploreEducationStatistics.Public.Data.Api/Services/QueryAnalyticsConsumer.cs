using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services;

public class QueryAnalyticsConsumer(
    IAnalyticsManager manager,
    IQueryAnalyticsWriter queryAnalyticsWriter,
    ILogger<QueryAnalyticsConsumer> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)  
        {
            try
            {
                var message = await manager.ReadQuery(stoppingToken);
                await queryAnalyticsWriter.ReportDataSetVersionQuery(message as CaptureDataSetVersionQueryRequest); // @MarkFix
            }
            catch (Exception e)
            {
                logger.LogError(
                    exception: e,
                    message: "Error whilst reading a query from {QueryManager}",
                    nameof(IAnalyticsManager));
            }
        }       
    }
}
