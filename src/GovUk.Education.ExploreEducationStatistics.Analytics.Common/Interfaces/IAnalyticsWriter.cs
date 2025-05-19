namespace GovUk.Education.ExploreEducationStatistics.Analytics.Common.Interfaces;

public interface IAnalyticsWriter
{
    Task Report(IAnalyticsCaptureRequestBase request, CancellationToken cancellationToken);
}
