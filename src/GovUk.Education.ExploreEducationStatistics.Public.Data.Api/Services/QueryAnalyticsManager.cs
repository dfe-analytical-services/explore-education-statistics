using System.Threading.Channels;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services;

public class QueryAnalyticsManager : IQueryAnalyticsManager
{
    private readonly Channel<CaptureDataSetVersionQueryRequest> _channel = 
        Channel.CreateUnbounded<CaptureDataSetVersionQueryRequest>();

    public async Task AddQuery(CaptureDataSetVersionQueryRequest request, CancellationToken cancellationToken)
    {
        await _channel.Writer.WriteAsync(request, cancellationToken);
    }

    public ValueTask<CaptureDataSetVersionQueryRequest> ReadQuery(CancellationToken cancellationToken)
    {
        return _channel.Reader.ReadAsync(cancellationToken);
    }
}

public class NoOpQueryAnalyticsManager : IQueryAnalyticsManager
{
    public Task AddQuery(CaptureDataSetVersionQueryRequest request, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public async ValueTask<CaptureDataSetVersionQueryRequest> ReadQuery(CancellationToken cancellationToken)
    {
        await Task.Delay(Timeout.Infinite, cancellationToken);
        return default!;
    }
}
