namespace GovUk.Education.ExploreEducationStatistics.Common.Utils.Interfaces;

/// <summary>
/// An interface mimicking PeriodicTimer's public methods, so that we can mock it in tests.
/// </summary>
public interface IPeriodicTimer : IDisposable
{
    ValueTask<bool> WaitForNextTickAsync(CancellationToken cancellationToken);
}
