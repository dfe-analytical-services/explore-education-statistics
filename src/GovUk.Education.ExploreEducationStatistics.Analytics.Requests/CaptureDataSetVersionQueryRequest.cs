namespace GovUk.Education.ExploreEducationStatistics.Analytics.Model;

public record CaptureDataSetVersionQueryRequest(
    Guid dataSetId,
    Guid dataSetVersionId,
    string dataSetVersion,
    int resultsCount,
    int totalRowsCount,
    DateTime startTime,
    DateTime endTime,
    string query);
