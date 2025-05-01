#nullable enable
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Requests;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services;

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
