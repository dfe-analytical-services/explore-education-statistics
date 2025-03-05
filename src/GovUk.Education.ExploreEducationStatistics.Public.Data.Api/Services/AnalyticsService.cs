using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Utils;
using Newtonsoft.Json;
using IAnalyticsPathResolver = GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces.IAnalyticsPathResolver;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services;

public class AnalyticsService(
    IAnalyticsPathResolver analyticsPathResolver,
    ILogger<AnalyticsService> logger) : IAnalyticsService
{
    public async Task ReportDataSetVersionQuery(
        Guid dataSetId,
        Guid dataSetVersionId,
        string semVersion,
        string dataSetTitle,
        DataSetQueryRequest query,
        int resultsCount,
        int totalRowsCount,
        DateTime startTime,
        DateTime endTime)
    {
        logger.LogInformation(
            "Capturing query for analytics for data set {DataSetTitle}",
            dataSetTitle);

        var request = new CaptureDataSetVersionQueryRequest(
            DataSetId: dataSetId,
            DataSetVersionId: dataSetVersionId,
            DataSetVersion: semVersion,
            DataSetTitle: dataSetTitle,
            ResultsCount: resultsCount,
            TotalRowsCount: totalRowsCount,
            StartTime: startTime,
            EndTime: endTime,
            Query: DataSetQueryNormalisationUtil.NormaliseQuery(query));

        var serialisedRequest = JsonSerializationUtils.Serialize(
            obj: request,
            formatting: Formatting.Indented,
            orderedProperties: true,
            camelCase: true);

        var directory = analyticsPathResolver.PublicApiQueriesDirectoryPath();

        Directory.CreateDirectory(directory);

        var filename = $"{DateTime.UtcNow:yyyyMMdd-HHmmss-fff}_{dataSetVersionId}.json";
        
        await File.WriteAllTextAsync(
            Path.Combine(directory, filename),
            contents: serialisedRequest);
    }
}

internal record CaptureDataSetVersionQueryRequest(
    Guid DataSetId,
    Guid DataSetVersionId,
    string DataSetVersion,
    string DataSetTitle,
    int ResultsCount,
    int TotalRowsCount,
    DateTime StartTime,
    DateTime EndTime,
    DataSetQueryRequest Query);
