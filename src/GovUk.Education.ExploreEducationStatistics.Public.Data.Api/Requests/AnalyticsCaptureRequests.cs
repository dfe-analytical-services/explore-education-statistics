using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;

public record CaptureDataSetVersionQueryRequest(
    Guid DataSetId,
    Guid DataSetVersionId,
    string DataSetVersion,
    string DataSetTitle,
    int ResultsCount,
    int TotalRowsCount,
    DateTime StartTime,
    DateTime EndTime,
    DataSetQueryRequest Query) : IAnalyticsCaptureRequestBase;

public record CaptureDataSetVersionCallRequest(
    Guid DataSetId,
    Guid DataSetVersionId,
    string DataSetVersion,
    string DataSetTitle,
    PreviewTokenRequest? PreviewToken,
    string? RequestedDataSetVersion,
    DateTimeOffset StartTime,
    [property:JsonConverter(typeof(StringEnumConverter))]
    DataSetVersionCallType Type,
    object? Parameters = null
) : IAnalyticsCaptureRequestBase;

public enum DataSetVersionCallType
{
    GetMetadata,
    GetSummary,
    DownloadCsv,
    GetChanges
}

public record PreviewTokenRequest(
    string Label,
    Guid DataSetVersionId,
    DateTimeOffset Created,
    DateTimeOffset Expiry);
