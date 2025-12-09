using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.Cdn;
using Azure.ResourceManager.Cdn.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Options;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class CdnService(IOptions<CdnOptions> options, ILogger<CdnService> logger) : ICdnService
{
    private readonly CdnOptions _options = options.Value;
    private readonly ArmClient _client = new(new DefaultAzureCredential());

    public async Task PurgeCachePaths(string[] paths)
    {
        var endpointResourceId = new ResourceIdentifier(_options.EndpointResourceId);
        var endpoint = _client.GetFrontDoorEndpointResource(endpointResourceId);

        var content = new FrontDoorPurgeContent(paths);

        content.Domains.Add(_options.EesDomain);
        content.Domains.Add(_options.DefaultAfdDomain);

        await endpoint.PurgeContentAsync(WaitUntil.Started, content);

        logger.LogInformation($"CDN Paths purged: {paths}");
    }

    public async Task PurgeReleaseAndSubpages(string publicationSlug, string releaseSlug)
    {
        var releasePath = $"find-statistics/{publicationSlug}/{releaseSlug}";
        string[] paths =
        [
            $"/{releasePath}",
            $"/{releasePath}/*",
            $"/_next/data/{_options.NextJsBuildId}/{releasePath}.json",
            $"/_next/data/{_options.NextJsBuildId}/{releasePath}/*",
        ];

        await PurgeCachePaths(paths);
    }
}

internal class NoOpCdnService(ILogger<NoOpCdnService> logger) : ICdnService
{
    public Task PurgeCachePaths(string[] paths)
    {
        logger.LogInformation($"CDN Paths purged: {paths}");
        return Task.CompletedTask;
    }

    public Task PurgeCachePath(string path)
    {
        logger.LogInformation($"CDN Path purged: {path}");
        return Task.CompletedTask;
    }

    public Task PurgeReleaseAndSubpages(string publicationSlug, string releaseSlug)
    {
        logger.LogInformation($"CDN release purged: {publicationSlug}, {releaseSlug}");
        return Task.CompletedTask;
    }
}
