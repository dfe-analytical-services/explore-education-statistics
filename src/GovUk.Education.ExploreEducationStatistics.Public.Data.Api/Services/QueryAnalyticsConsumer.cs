using System.Threading.Channels;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services;

public class QueryAnalyticsConsumer(
    IQueryAnalyticsChannel channel,
    IQueryAnalyticsWriter queryAnalyticsWriter
    ) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)  
        {            
            var message = await channel.ReadQuery(stoppingToken);  
            await queryAnalyticsWriter.ReportDataSetVersionQuery(message);  
        }       
    }
}
