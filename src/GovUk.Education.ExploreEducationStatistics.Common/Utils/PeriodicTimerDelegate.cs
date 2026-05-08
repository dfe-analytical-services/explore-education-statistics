using GovUk.Education.ExploreEducationStatistics.Common.Utils.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Common.Utils;

/// <summary>
/// A delegate around a real PeriodicTimer, used in production code to supply real
/// PeriodicTimer functionality whilst also allowing tests to mock IPeriodicTimer.
/// </summary>
public class PeriodicTimerDelegate(TimeSpan timeSpan) : IPeriodicTimer
{
    private readonly PeriodicTimer _timer = new(timeSpan);

    public ValueTask<bool> WaitForNextTickAsync(CancellationToken cancellationToken = default) =>
        _timer.WaitForNextTickAsync(cancellationToken);

    public void Dispose()
    {
        _timer.Dispose();
    }
}
