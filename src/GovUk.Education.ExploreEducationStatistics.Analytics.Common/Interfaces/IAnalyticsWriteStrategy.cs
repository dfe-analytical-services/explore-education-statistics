namespace GovUk.Education.ExploreEducationStatistics.Analytics.Common.Interfaces;

public interface IAnalyticsWriteStrategy
{
    Type RequestType { get; }

    Task Report(IAnalyticsCaptureRequest request, CancellationToken cancellationToken);
}
