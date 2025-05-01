namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;

public record AnalyticsCaptureRequestBase();

public record CaptureDataSetVersionQueryRequest(
    Guid DataSetId,
    Guid DataSetVersionId,
    string DataSetVersion,
    string DataSetTitle,
    int ResultsCount,
    int TotalRowsCount,
    DateTime StartTime,
    DateTime EndTime,
    DataSetQueryRequest Query) : AnalyticsCaptureRequestBase;
