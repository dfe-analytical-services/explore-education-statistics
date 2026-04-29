using GovUk.Education.ExploreEducationStatistics.Common.Utils.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Common.Utils;

public class PeriodicTimerWrapper(TimeSpan timeSpan) : IPeriodicTimer
{
    private readonly PeriodicTimer _timer = new(timeSpan);

    public ValueTask<bool> WaitForNextTickAsync(CancellationToken cancellationToken = default) =>
        _timer.WaitForNextTickAsync(cancellationToken);

    public void Dispose()
    {
        _timer.Dispose();
    }
}
