using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.ContentApi;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.HealthChecks.Strategies;

internal class ContentApiHealthCheckStrategy(
    IOptions<ContentApiOptions> options,
    ILogger<ContentApiHealthCheckStrategy> logger,
    Func<IContentApiClient> contentApiClientFactory) : IHealthCheckStrategy
{
    public async Task<HealthCheckResult> Run(CancellationToken cancellationToken)
    {
        logger.LogInformation("Running Content API health check");
        
        if (!options.Value.IsValid(out var errorMessage))
        {
            logger.LogWarning("Content API health check failed: Provider options are not valid. {@Options}", options.Value);
            return new HealthCheckResult(false,  errorMessage);
        }
        
        logger.LogInformation("Making Ping call to Content API...");
        var contentApiClient = contentApiClientFactory();
        var pingResult = await contentApiClient.Ping(cancellationToken);

        var resultMessage = pingResult.WasSuccesssful
            ? "Connection to Content API:OK"
            : pingResult.ErrorMessage ?? "Connection to Content API:Failed";
        
        logger.LogInformation("Result:{ResultMessage}", resultMessage);
        
        return new(
            pingResult.WasSuccesssful, 
            resultMessage);
    }
}
