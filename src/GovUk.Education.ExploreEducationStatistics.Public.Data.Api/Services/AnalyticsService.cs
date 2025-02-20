using System.Diagnostics;
using GovUk.Education.ExploreEducationStatistics.Analytics.Service.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Utils;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services;

public class AnalyticsService(
    IAnalyticsPathResolver analyticsPathResolver,
    ILogger<AnalyticsService> logger) : IAnalyticsService
{
    private static readonly Stopwatch Stopwatch = new();
    
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

        var serialisedRequest = JsonSerializationUtils.SerializeWithOrderedProperties(
            obj: request,
            formatting: Formatting.Indented);

        var directory = analyticsPathResolver.PublicApiQueriesDirectoryPath();

        Directory.CreateDirectory(directory);

        var filename = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}_{dataSetVersionId}.json";
        
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
