using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Utils;
using Newtonsoft.Json;
using IAnalyticsPathResolver = GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces.IAnalyticsPathResolver;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services;

public class QueryAnalyticsWriter(
    IAnalyticsPathResolver analyticsPathResolver,
    ILogger<QueryAnalyticsWriter> logger) : IQueryAnalyticsWriter
{
    public async Task ReportDataSetVersionQuery(CaptureDataSetVersionQueryRequest request)
    {
        logger.LogInformation(
            "Capturing query for analytics for data set {DataSetTitle}",
            request.DataSetTitle);

        var serialisedRequest = JsonSerializationUtils.Serialize(
            obj: request with
            {
                Query = DataSetQueryNormalisationUtil.NormaliseQuery(request.Query)
            },
            formatting: Formatting.Indented,
            orderedProperties: true,
            camelCase: true);

        var directory = analyticsPathResolver.PublicApiQueriesDirectoryPath();

        try
        {
            Directory.CreateDirectory(directory);

            var filename = $"{DateTime.UtcNow:yyyyMMdd-HHmmss-fff}_{request.DataSetVersionId}.json";

            await File.WriteAllTextAsync(
                Path.Combine(directory, filename),
                contents: serialisedRequest);
        }
        catch (Exception e)
        {
            logger.LogError(
                exception: e,
                message: "Error whilst writing {QueryRequest} to disk",
                nameof(CaptureDataSetVersionQueryRequest));
        }
    }
}

public record CaptureDataSetVersionQueryRequest(
    Guid DataSetId,
    Guid DataSetVersionId,
    string DataSetVersion,
    string DataSetTitle,
    int ResultsCount,
    int TotalRowsCount,
    DateTime StartTime,
    DateTime EndTime,
    DataSetQueryRequest Query);
