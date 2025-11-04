using System.Threading.Channels;
using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Analytics.Common;

public class AnalyticsManager : IAnalyticsManager
{
    private readonly Channel<IAnalyticsCaptureRequest> _channel = Channel.CreateUnbounded<IAnalyticsCaptureRequest>();

    public async Task Add(IAnalyticsCaptureRequest request, CancellationToken cancellationToken)
    {
        await _channel.Writer.WriteAsync(request, cancellationToken);
    }

    public ValueTask<IAnalyticsCaptureRequest> Read(CancellationToken cancellationToken)
    {
        return _channel.Reader.ReadAsync(cancellationToken);
    }
}

public class NoOpAnalyticsManager : IAnalyticsManager
{
    public Task Add(IAnalyticsCaptureRequest request, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public async ValueTask<IAnalyticsCaptureRequest> Read(CancellationToken cancellationToken)
    {
        await Task.Delay(Timeout.Infinite, cancellationToken);
        return default!;
    }
}
