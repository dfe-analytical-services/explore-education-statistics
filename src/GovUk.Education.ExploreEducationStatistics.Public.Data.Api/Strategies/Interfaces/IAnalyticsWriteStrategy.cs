using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Strategies.Interfaces;

public interface IAnalyticsWriteStrategyBase
{
    Type RequestType { get; }
}

public interface IAnalyticsWriteStrategy<in TRequest> : IAnalyticsWriteStrategyBase
    where TRequest : IAnalyticsCaptureRequestBase
{
    string GetDirectory();

    string GetFilename(TRequest request);

    string SerialiseRequest(TRequest request);
}
