#nullable enable
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Requests;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services;

public class AnalyticsManager : IAnalyticsManager
{
    private readonly Channel<IAnalyticsCaptureRequestBase> _channel =
        Channel.CreateUnbounded<IAnalyticsCaptureRequestBase>();

    public async Task Add(IAnalyticsCaptureRequestBase request, CancellationToken cancellationToken)
    {
        await _channel.Writer.WriteAsync(request, cancellationToken);
    }

    public ValueTask<IAnalyticsCaptureRequestBase> Read(CancellationToken cancellationToken)
    {
        return _channel.Reader.ReadAsync(cancellationToken);
    }
}

public class NoOpAnalyticsManager : IAnalyticsManager
{
    public Task Add(IAnalyticsCaptureRequestBase request, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public async ValueTask<IAnalyticsCaptureRequestBase> Read(CancellationToken cancellationToken)
    {
        await Task.Delay(Timeout.Infinite, cancellationToken);
        return default!;
    }
}
