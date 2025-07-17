namespace GovUk.Education.ExploreEducationStatistics.Analytics.Common.Interfaces;

public interface IAnalyticsManager
{
    Task Add(IAnalyticsCaptureRequest request, CancellationToken cancellationToken);

    ValueTask<IAnalyticsCaptureRequest> Read(CancellationToken cancellationToken);
}
