using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.AzureBlobStorage;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Options;
using Microsoft.Extensions.Options;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.HealthChecks.Strategies;

internal class AzureBlobStorageHealthCheckStrategy(
    IAzureBlobStorageClient azureBlobStorageClient,
    IOptions<AppOptions> appOptions) : IHealthCheckStrategy
{
    public async Task<HealthCheckResult> Run(CancellationToken cancellationToken)
    {
        var containerName = appOptions.Value.SearchableDocumentsContainerName;
        if (string.IsNullOrWhiteSpace(containerName))
        {
            return new HealthCheckResult(false,
                $"Azure blob storage container name is not specified in {AppOptions.Section}.{nameof(AppOptions.SearchableDocumentsContainerName)}");
        }

        try
        {
            var containerExists = await azureBlobStorageClient.ContainerExists(containerName, cancellationToken);
            if (!containerExists)
            {
                return new HealthCheckResult(false,
                    $"Azure blob storage container '{containerName}' is not found");
            }

        }
        catch (Exception e)
        {
            return new HealthCheckResult(false, $"Error occurred whilst trying to check for Azure blob storage container '{containerName}': {e.Message}");
        }
        
        return new HealthCheckResult(true);
    }
}
