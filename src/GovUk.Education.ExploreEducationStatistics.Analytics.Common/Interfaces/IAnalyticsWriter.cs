namespace GovUk.Education.ExploreEducationStatistics.Analytics.Common.Interfaces;

public interface IAnalyticsWriter
{
    Task Report(IAnalyticsCaptureRequest request, CancellationToken cancellationToken);
}
