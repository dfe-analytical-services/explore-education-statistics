using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.ContentApi;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Options;
using Microsoft.Extensions.Options;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.HealthChecks.Strategies;

internal class ContentApiHealthCheckStrategy(
    IOptions<ContentApiOptions> options,
    Func<IContentApiClient> contentApiClientFactory) : IHealthCheckStrategy
{
    public async Task<HealthCheckResult> Run(CancellationToken cancellationToken)
    {
        if (!options.Value.IsValid(out var errorMessage))
        {
            return new HealthCheckResult(false,  errorMessage);
        }
        var contentApiClient = contentApiClientFactory();
        var pingResult = await contentApiClient.Ping(cancellationToken);
        
        return new(
            pingResult.WasSuccesssful, 
            pingResult.WasSuccesssful
                ? "Connection to Content API:OK"
                : pingResult.ErrorMessage ?? "Connection to Content API:Failed");
    }
}
