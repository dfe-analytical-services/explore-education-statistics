using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;

public interface IAnalyticsService
{
    Task CaptureTopLevelCall(
        TopLevelCallType type,
        object? parameters = null,
        CancellationToken cancellationToken = default);
    
    Task CaptureDataSetCall(
        Guid dataSetId,
        DataSetCallType type,
        object? parameters = null,
        CancellationToken cancellationToken = default);
    
    Task CaptureDataSetVersionCall(
        Guid dataSetVersionId,
        DataSetVersionCallType type,
        string? requestedDataSetVersion,
        object? parameters = null,
        CancellationToken cancellationToken = default);

    Task CaptureDataSetVersionQuery(
        DataSetVersion dataSetVersion,
        string? requestedDataSetVersion,
        DataSetQueryRequest query,
        DataSetQueryPaginatedResultsViewModel results,
        DateTime startTime,
        CancellationToken cancellationToken);
}
