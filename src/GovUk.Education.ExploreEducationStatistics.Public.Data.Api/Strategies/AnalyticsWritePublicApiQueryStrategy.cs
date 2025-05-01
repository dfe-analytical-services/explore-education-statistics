using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Utils;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Strategies.Interfaces;
using Newtonsoft.Json;
using IAnalyticsPathResolver = GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces.IAnalyticsPathResolver;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Strategies;

public class AnalyticsWritePublicApiQueryStrategy(
    IAnalyticsPathResolver analyticsPathResolver,
    ILogger<AnalyticsWritePublicApiQueryStrategy> logger) : IAnalyticsWriteStrategy
{
    public async Task Report(AnalyticsCaptureRequestBase request, CancellationToken cancellationToken)
    {
        var queryRequest = request as CaptureDataSetVersionQueryRequest
                           ?? throw new ArgumentException(
                               $"request isn't a {nameof(CaptureDataSetVersionQueryRequest)}");
        logger.LogInformation(
            "Capturing query for analytics for data set {DataSetTitle}",
            queryRequest.DataSetTitle);

        var serialisedRequest = JsonSerializationUtils.Serialize(
            obj: queryRequest with
            {
                Query = DataSetQueryNormalisationUtil.NormaliseQuery(queryRequest.Query)
            },
            formatting: Formatting.Indented,
            orderedProperties: true,
            camelCase: true);

        var directory = analyticsPathResolver.PublicApiQueriesDirectoryPath();

        try
        {
            Directory.CreateDirectory(directory);

            var filename =
                $"{DateTime.UtcNow:yyyyMMdd-HHmmss}_{queryRequest.DataSetVersionId}_{RandomUtils.RandomString()}.json";

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
