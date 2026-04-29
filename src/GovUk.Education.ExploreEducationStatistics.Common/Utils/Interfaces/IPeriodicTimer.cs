namespace GovUk.Education.ExploreEducationStatistics.Common.Utils.Interfaces;

public interface IPeriodicTimer : IDisposable
{
    ValueTask<bool> WaitForNextTickAsync(CancellationToken cancellationToken);
}
