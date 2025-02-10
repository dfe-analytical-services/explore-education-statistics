using GovUk.Education.ExploreEducationStatistics.Analytics.Model;
using GovUk.Education.ExploreEducationStatistics.Analytics.Requests.Consumer.Services;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services;

public class AnalyticsService(
    IAnalyticsPathResolver analyticsPathResolver,
    ILogger<AnalyticsService> logger) : IAnalyticsService
{
    public async Task ReportDataSetVersionQuery(
        DataSetVersion dataSetVersion,
        DataSetQueryRequest query,
        int resultsCount,
        int totalRowsCount,
        DateTime startTime,
        DateTime endTime)
    {
        logger.LogInformation(
            "Capturing query for analytics for data set {DataSetTitle}",
            dataSetVersion.DataSet.Title);

        var queryJson = JsonConvert.SerializeObject(query);
        
        var request = new CaptureDataSetVersionQueryRequest(
            DataSetId: dataSetVersion.DataSetId,
            DataSetVersionId: dataSetVersion.Id,
            DataSetVersion: dataSetVersion.SemVersion().ToString(),
            DataSetTitle: dataSetVersion.DataSet.Title,
            ResultsCount: resultsCount,
            TotalRowsCount: totalRowsCount,
            StartTime: startTime,
            EndTime: endTime,
            Query: queryJson);

        var directory = analyticsPathResolver.PublicApiQueriesDirectoryPath();

        Directory.CreateDirectory(directory);
        
        await File.WriteAllTextAsync(
            Path.Combine(directory, $"{Guid.NewGuid()}.json"), 
            contents: JsonConvert.SerializeObject(request));
    }
}
