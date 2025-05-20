using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Strategies.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services;

public class AnalyticsWriter(
    IEnumerable<IAnalyticsWriteStrategyBase> strategies,
    ILogger<AnalyticsWriter> logger) : IAnalyticsWriter
{
    private readonly Dictionary<Type, IAnalyticsWriteStrategyBase> _strategyByRequestType =
        strategies.ToDictionary(strategy => strategy.RequestType);

    public async Task Report(IAnalyticsCaptureRequestBase request, CancellationToken cancellationToken)
    {
        if (!_strategyByRequestType.TryGetValue(request.GetType(), out var strategy))
        {
            throw new Exception($"No write strategy for request type {request.GetType()}");
        }

        var genericStrategy = strategy as IAnalyticsWriteStrategy<IAnalyticsCaptureRequestBase>
                              ?? throw new Exception("WAAAA!");

        logger.LogInformation(
            "Capturing query for request {Request}",
            request);

        var directory = genericStrategy.GetDirectory();
        var filename = genericStrategy.GetFilename(request);

        try
        {
            Directory.CreateDirectory(directory);

            var filePath = Path.Combine(directory, filename);

            var serialisedRequest = genericStrategy.SerialiseRequest(request);

            await File.WriteAllTextAsync(filePath, contents: serialisedRequest, cancellationToken: cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(
                e,
                "Error whilst writing {RequestTypeName} to disk",
                request.GetType().ToString());
        }
    }
}
