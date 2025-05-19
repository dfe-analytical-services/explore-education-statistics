namespace GovUk.Education.ExploreEducationStatistics.Analytics.Common.Interfaces;

public interface IAnalyticsWriteStrategy
{
    Type RequestType { get; }

    Task Report(IAnalyticsCaptureRequestBase request, CancellationToken cancellationToken);
}
