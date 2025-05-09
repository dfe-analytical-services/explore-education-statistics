using System.Threading.Channels;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services;

public class AnalyticsManager : IAnalyticsManager
{
    private readonly Channel<AnalyticsCaptureRequestBase> _channel =
        Channel.CreateUnbounded<AnalyticsCaptureRequestBase>();

    public async Task Add(AnalyticsCaptureRequestBase request, CancellationToken cancellationToken)
    {
        await _channel.Writer.WriteAsync(request, cancellationToken);
    }

    public ValueTask<AnalyticsCaptureRequestBase> Read(CancellationToken cancellationToken)
    {
        return _channel.Reader.ReadAsync(cancellationToken);
    }
}

public class NoOpAnalyticsManager : IAnalyticsManager
{
    public Task Add(AnalyticsCaptureRequestBase request, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public async ValueTask<AnalyticsCaptureRequestBase> Read(CancellationToken cancellationToken)
    {
        await Task.Delay(Timeout.Infinite, cancellationToken);
        return default!;
    }
}
