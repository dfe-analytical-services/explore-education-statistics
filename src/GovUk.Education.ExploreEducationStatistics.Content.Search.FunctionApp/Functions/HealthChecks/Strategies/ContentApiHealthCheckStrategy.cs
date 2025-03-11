using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.ContentApi;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.HealthChecks.Strategies;

internal class ContentApiHealthCheckStrategy(IContentApiClient contentApiClient) : IHealthCheckStrategy
{
    public async Task<HealthCheckResult> Run(CancellationToken cancellationToken)
    {
        var pingResult = await contentApiClient.Ping(cancellationToken);
        return new HealthCheckResult(pingResult.WasSuccesssful, pingResult.ErrorMessage);
    }
}
