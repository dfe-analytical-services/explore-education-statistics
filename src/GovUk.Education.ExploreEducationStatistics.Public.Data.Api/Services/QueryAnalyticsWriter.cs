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

        var filename = $"{DateTime.UtcNow:yyyyMMdd-HHmmss-fffffff}_{request.DataSetVersionId}.json";

        try
        {
            Directory.CreateDirectory(directory);

            var filePath = Path.Combine(directory, filename);
            if (File.Exists(filePath))
            {
                throw new Exception($"Tried to create a file that already exists for {nameof(CaptureDataSetVersionQueryRequest)}!");
            }

            await File.WriteAllTextAsync(
                filePath,
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
