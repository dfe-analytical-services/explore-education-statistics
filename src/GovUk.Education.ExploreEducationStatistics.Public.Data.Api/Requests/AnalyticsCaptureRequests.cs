using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;

public record CaptureTopLevelCallRequest(
    DateTimeOffset StartTime,
    [property: JsonConverter(typeof(StringEnumConverter))]
    TopLevelCallType Type,
    object? Parameters = null
) : IAnalyticsCaptureRequest;

public record CapturePublicationCallRequest(
    Guid PublicationId,
    string PublicationTitle,
    DateTimeOffset StartTime,
    [property:JsonConverter(typeof(StringEnumConverter))]
    PublicationCallType Type,
    object? Parameters = null
) : IAnalyticsCaptureRequest;

public record CaptureDataSetCallRequest(
    Guid DataSetId,
    string DataSetTitle,
    PreviewTokenRequest? PreviewToken,
    DateTimeOffset StartTime,
    [property:JsonConverter(typeof(StringEnumConverter))]
    DataSetCallType Type,
    object? Parameters = null
) : IAnalyticsCaptureRequest;

public record CaptureDataSetVersionQueryRequest(
    Guid DataSetId,
    Guid DataSetVersionId,
    string DataSetVersion,
    string DataSetTitle,
    PreviewTokenRequest? PreviewToken,
    string? RequestedDataSetVersion,
    int ResultsCount,
    int TotalRowsCount,
    DateTime StartTime,
    DateTime EndTime,
    DataSetQueryRequest Query) : IAnalyticsCaptureRequest;

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
) : IAnalyticsCaptureRequest;

public enum TopLevelCallType
{
    GetPublications
}

public enum PublicationCallType
{
    GetSummary,
    GetDataSets
}

public enum DataSetCallType
{
    GetSummary,
    GetVersions
}

public enum DataSetVersionCallType
{
    GetMetadata,
    GetSummary,
    DownloadCsv,
    GetChanges
}

public record GetMetadataAnalyticsParameters(
    [property:JsonProperty(ItemConverterType=typeof(StringEnumConverter))]
    IEnumerable<DataSetMetaType> Types);

public record PreviewTokenRequest(
    string Label,
    Guid DataSetVersionId,
    DateTimeOffset Created,
    DateTimeOffset Expiry);

public record PaginationParameters(
    int Page,
    int PageSize);
