using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.AzureBlobStorage;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.HealthChecks.Strategies;

internal class AzureBlobStorageHealthCheckStrategy(
    Func<IAzureBlobStorageClient> azureBlobStorageClientFactory,
    ILogger<AzureBlobStorageHealthCheckStrategy> logger,
    IOptions<AppOptions> appOptions) : IHealthCheckStrategy
{
    public async Task<HealthCheckResult> Run(CancellationToken cancellationToken)
    {
        logger.LogInformation("Running Azure blob storage health check");
        
        if (!appOptions.Value.IsValid(out var errorMessage))
        {
            logger.LogWarning("Azure blob storage health check failed: Provider options are not valid. {@Options}", appOptions.Value);
            return new HealthCheckResult(false,  errorMessage);
        }
        
        var containerName = appOptions.Value.SearchableDocumentsContainerName;
        if (string.IsNullOrWhiteSpace(containerName))
        {
            logger.LogWarning("Azure blob storage health check failed: Container name is not specified in configuration.");
            return new HealthCheckResult(false,
                $"Azure blob storage container name is not specified in {AppOptions.Section}.{nameof(AppOptions.SearchableDocumentsContainerName)}");
        }

        try
        {
            logger.LogInformation("Attempting to connect to Azure blob storage container '{ContainerName}'...", containerName);
            var  azureBlobStorageClient = azureBlobStorageClientFactory();
            var containerExists = await azureBlobStorageClient.ContainerExists(containerName, cancellationToken);
            if (!containerExists)
            {
                logger.LogWarning("Azure blob storage container '{ContainerName}' is not found.", containerName);
                return new HealthCheckResult(false,
                    $"Azure blob storage container '{containerName}' is not found");
            }

        }
        catch (Exception e)
        {
            logger.LogError(e, "Error occurred whilst trying to check for Azure blob storage container '{ContainerName}': {Message}", containerName, e.Message);
            return new HealthCheckResult(false, $"Error occurred whilst trying to check for Azure blob storage container '{containerName}': {e.Message}");
        }

        logger.LogInformation("Healthcheck was successful: Azure blob storage container '{ContainerName}' is found.", containerName);
        return new HealthCheckResult(true, "Connection to Azure blob storage container:OK");
    }
}
