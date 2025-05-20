using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;

public interface IAnalyticsService
{
    Task CaptureDataSetVersionCall(
        Guid dataSetVersionId,
        DataSetVersionCallType type,
        string? requestedDataSetVersion,
        object? parameters = null,
        CancellationToken cancellationToken = default);
}
