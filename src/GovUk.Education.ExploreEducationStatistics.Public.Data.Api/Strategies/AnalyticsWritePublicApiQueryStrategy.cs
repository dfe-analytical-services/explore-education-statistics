using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Utils;
using Newtonsoft.Json;
using IAnalyticsPathResolver = GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces.IAnalyticsPathResolver;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Strategies;

public class AnalyticsWritePublicApiQueryStrategy(
    IAnalyticsPathResolver analyticsPathResolver,
    DateTimeProvider dateTimeProvider,
    ILogger<AnalyticsWritePublicApiQueryStrategy> logger
    ) : IAnalyticsWriteStrategy
{
    public Type RequestType => typeof(CaptureDataSetVersionQueryRequest);

    public async Task Report(IAnalyticsCaptureRequestBase request, CancellationToken cancellationToken)
    {
        if (request is not CaptureDataSetVersionQueryRequest queryRequest)
        {
            throw new ArgumentException($"request isn't a {nameof(CaptureDataSetVersionQueryRequest)}");
        }

        logger.LogInformation(
            "Capturing query for analytics for data set {DataSetTitle}",
            queryRequest.DataSetTitle);

        var requestToSerialise = queryRequest with { Query = DataSetQueryNormalisationUtil.NormaliseQuery(queryRequest.Query) };
        var directory = analyticsPathResolver.PublicApiQueriesDirectoryPath();
        var filename =
            $"{dateTimeProvider.UtcNow:yyyyMMdd-HHmmss}_{queryRequest.DataSetVersionId}_{RandomUtils.RandomString()}.json";

        try
        {
            Directory.CreateDirectory(directory);

            var filePath = Path.Combine(directory, filename);

            var serialisedRequest = JsonSerializationUtils.Serialize(
                obj: requestToSerialise,
                formatting: Formatting.Indented,
                orderedProperties: true,
                camelCase: true);

            await File.WriteAllTextAsync(filePath, contents: serialisedRequest, cancellationToken: cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(
                e,
                "Error whilst writing {RequestTypeName} to disk",
                request!.GetType().ToString());
        }
    }
}
