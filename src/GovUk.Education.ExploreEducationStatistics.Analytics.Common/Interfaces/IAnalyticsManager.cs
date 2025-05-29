namespace GovUk.Education.ExploreEducationStatistics.Analytics.Common.Interfaces;

public interface IAnalyticsManager
{
    Task Add(IAnalyticsCaptureRequestBase request, CancellationToken cancellationToken);

    ValueTask<IAnalyticsCaptureRequestBase> Read(CancellationToken cancellationToken);
}
