using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;

public interface IAnalyticsService
{
    Task CaptureTopLevelCall(
        TopLevelCallType type,
        object? parameters = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Favour the CapturePublicationCall method that accepts a
    /// "publicationTitle" parameter as this prevents the need to
    /// call the Content API in order to fetch additional Publication's
    /// details.
    /// </summary>
    Task CapturePublicationCall(
        Guid publicationId,
        PublicationCallType type,
        object? parameters = null,
        CancellationToken cancellationToken = default
    );

    Task CapturePublicationCall(
        Guid publicationId,
        string publicationTitle,
        PublicationCallType type,
        object? parameters = null,
        CancellationToken cancellationToken = default
    );

    Task CaptureDataSetCall(
        Guid dataSetId,
        DataSetCallType type,
        object? parameters = null,
        CancellationToken cancellationToken = default
    );

    Task CaptureDataSetVersionCall(
        Guid dataSetVersionId,
        DataSetVersionCallType type,
        string? requestedDataSetVersion,
        object? parameters = null,
        CancellationToken cancellationToken = default
    );

    Task CaptureDataSetVersionQuery(
        DataSetVersion dataSetVersion,
        string? requestedDataSetVersion,
        DataSetQueryRequest query,
        DataSetQueryPaginatedResultsViewModel results,
        DateTime startTime,
        CancellationToken cancellationToken
    );
}
