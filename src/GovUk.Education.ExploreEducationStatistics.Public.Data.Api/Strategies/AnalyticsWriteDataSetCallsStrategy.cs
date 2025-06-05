using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;
using Newtonsoft.Json;
using IAnalyticsPathResolver = GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces.IAnalyticsPathResolver;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Strategies;

public class AnalyticsWriteDataSetCallsStrategy(
    IAnalyticsPathResolver analyticsPathResolver,
    DateTimeProvider dateTimeProvider,
    ILogger<AnalyticsWriteDataSetCallsStrategy> logger
    ) : IAnalyticsWriteStrategy
{
    public Type RequestType => typeof(CaptureDataSetCallRequest);

    public async Task Report(IAnalyticsCaptureRequestBase request, CancellationToken cancellationToken)
    {
        if (request is not CaptureDataSetCallRequest callRequest)
        {
            throw new ArgumentException($"request isn't a {nameof(CaptureDataSetCallRequest)}");
        }
        
        logger.LogInformation(
            "Capturing DataSet-level call of type {Type} for analytics - for data set {DataSetTitle}",
            callRequest.Type,
            callRequest.DataSetTitle);

        var directory = analyticsPathResolver.PublicApiDataSetCallsDirectoryPath();
        var filename =
            $"{dateTimeProvider.UtcNow:yyyyMMdd-HHmmss}_{callRequest.DataSetId}_{RandomUtils.RandomString()}.json";

        try
        {
            Directory.CreateDirectory(directory);

            var filePath = Path.Combine(directory, filename);

            var serialisedRequest = JsonSerializationUtils.Serialize(
                obj: callRequest,
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
                callRequest.GetType().ToString());
        }
    }
}
