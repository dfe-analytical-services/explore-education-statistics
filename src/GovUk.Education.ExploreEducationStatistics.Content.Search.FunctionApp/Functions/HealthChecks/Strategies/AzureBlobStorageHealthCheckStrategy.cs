using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.AzureBlobStorage;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.HealthChecks.Strategies;

internal class AzureBlobStorageHealthCheckStrategy(
    Func<IAzureBlobStorageClient> azureBlobStorageClientFactory,
    ILogger<AzureBlobStorageHealthCheckStrategy> logger,
    IOptions<AppOptions> appOptions
) : IHealthCheckStrategy
{
    public string Description => "Search Documents Azure Blob Storage container check";

    public async Task<HealthCheckResult> Run(CancellationToken cancellationToken)
    {
        logger.LogInformation("Running Search Documents Azure Blob Storage container health check");

        if (!appOptions.Value.IsValid(out var errorMessage))
        {
            logger.LogWarning(
                "Search Documents Azure Blob Storage container health check failed: Provider options are not valid. {@Options}",
                appOptions.Value
            );
            return new HealthCheckResult(this, false, errorMessage);
        }

        var containerName = appOptions.Value.SearchDocumentsContainerName;
        if (string.IsNullOrWhiteSpace(containerName))
        {
            logger.LogWarning(
                "Search Documents Azure Blob Storage container health check failed: Search Documents container name is not specified in configuration."
            );
            return new HealthCheckResult(
                this,
                false,
                $"Search Documents Azure Blob Storage container name is not specified in {AppOptions.Section}.{nameof(AppOptions.SearchDocumentsContainerName)}"
            );
        }

        try
        {
            logger.LogInformation(
                "Attempting to connect to Search Documents Azure Blob Storage container '{ContainerName}'...",
                containerName
            );
            var azureBlobStorageClient = azureBlobStorageClientFactory();
            var containerExists = await azureBlobStorageClient.ContainerExists(containerName, cancellationToken);
            if (!containerExists)
            {
                logger.LogWarning(
                    "Search Documents Azure Blob Storage container '{ContainerName}' is not found.",
                    containerName
                );
                return new HealthCheckResult(
                    this,
                    false,
                    $"Search Documents Azure Blob Storage container '{containerName}' is not found"
                );
            }
        }
        catch (Exception e)
        {
            logger.LogError(
                e,
                "Error occurred whilst trying to check for Search Documents Azure Blob Storage container '{ContainerName}': {Message}",
                containerName,
                e.Message
            );
            return new HealthCheckResult(
                this,
                false,
                $"Error occurred whilst trying to check for Search Documents Azure Blob Storage container '{containerName}': {e.Message}"
            );
        }

        logger.LogInformation(
            "Health check was successful: Search Documents Azure Blob Storage container '{ContainerName}' is found.",
            containerName
        );
        return new HealthCheckResult(this, true, "Connection to Search Documents Azure Blob Storage container:OK");
    }
}
