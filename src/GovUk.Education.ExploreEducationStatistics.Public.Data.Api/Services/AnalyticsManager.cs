using System.Threading.Channels;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services;

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
