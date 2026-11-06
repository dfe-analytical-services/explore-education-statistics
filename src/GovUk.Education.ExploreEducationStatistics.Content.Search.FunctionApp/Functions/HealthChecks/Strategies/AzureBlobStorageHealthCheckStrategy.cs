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
    public string Description => "Azure Blob Storage containers check";

    public async Task<HealthCheckResult> Run(CancellationToken cancellationToken)
    {
        logger.LogInformation("Running Azure Blob Storage containers health check");

        if (!appOptions.Value.IsValid(out var errorMessage))
        {
            logger.LogWarning(
                "Azure Blob Storage containers health check failed: Provider options are not valid. {@Options}",
                appOptions.Value
            );
            return new HealthCheckResult(this, false, errorMessage);
        }

        (string Description, string ContainerName)[] containers =
        [
            ("Release Documents", appOptions.Value.ReleaseDocumentsContainerName),
            ("Dataset Documents", appOptions.Value.DatasetDocumentsContainerName),
        ];

        var azureBlobStorageClient = azureBlobStorageClientFactory();

        var results = new List<(bool IsHealthy, string Message)>();
        foreach (var (description, containerName) in containers)
        {
            results.Add(await CheckContainer(azureBlobStorageClient, description, containerName, cancellationToken));
        }

        return new HealthCheckResult(
            this,
            results.All(result => result.IsHealthy),
            string.Join(" ", results.Select(result => result.Message))
        );
    }

    private async Task<(bool IsHealthy, string Message)> CheckContainer(
        IAzureBlobStorageClient azureBlobStorageClient,
        string description,
        string containerName,
        CancellationToken cancellationToken
    )
    {
        try
        {
            logger.LogInformation(
                "Attempting to connect to {Description} Azure Blob Storage container '{ContainerName}'...",
                description,
                containerName
            );
            var containerExists = await azureBlobStorageClient.ContainerExists(containerName, cancellationToken);
            if (!containerExists)
            {
                logger.LogWarning(
                    "{Description} Azure Blob Storage container '{ContainerName}' is not found.",
                    description,
                    containerName
                );
                return (false, $"{description} Azure Blob Storage container '{containerName}' is not found.");
            }
        }
        catch (Exception e)
        {
            logger.LogError(
                e,
                "Error occurred whilst trying to check for {Description} Azure Blob Storage container '{ContainerName}': {Message}",
                description,
                containerName,
                e.Message
            );
            return (
                false,
                $"Error occurred whilst trying to check for {description} Azure Blob Storage container '{containerName}': {e.Message}"
            );
        }

        logger.LogInformation(
            "Health check was successful: {Description} Azure Blob Storage container '{ContainerName}' is found.",
            description,
            containerName
        );
        return (true, $"Connection to {description} Azure Blob Storage container '{containerName}':OK.");
    }
}
