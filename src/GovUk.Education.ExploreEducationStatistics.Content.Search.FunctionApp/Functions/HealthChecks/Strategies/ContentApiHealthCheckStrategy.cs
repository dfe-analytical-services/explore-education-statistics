using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.ContentApi;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.HealthChecks.Strategies;

internal class ContentApiHealthCheckStrategy(IContentApiClient contentApiClient) : IHeathCheckStrategy
{
    public async Task<HeathCheckResult> Run(CancellationToken cancellationToken)
    {
        var pingResult = await contentApiClient.Ping(cancellationToken);
        return new HeathCheckResult(pingResult.WasSuccesssful, pingResult.ErrorMessage);
    }
}
