using Azure.Storage.Blobs;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.AzureBlobStorage;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.HealthChecks.Strategies;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Options;
using Microsoft.Extensions.Logging.Abstractions;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Functions.HealthChecks.Strategies;

public class AzureBlobStorageHealthCheckStrategyIntegrationTests
{
    private const string StorageAccountName = "-- azure storage account name here --";
    private const string StorageAccountAccessKey = "-- azure storage account access key here --";

    private const string IntegrationTestStorageAccountConnectionString =
        $"AccountName={StorageAccountName};AccountKey={StorageAccountAccessKey};";
    private const string IntegrationTestContainerName = "integration-tests";

    private static AzureBlobStorageHealthCheckStrategy GetSUT(
        string? searchStorageConnectionString = null,
        string? searchableDocumentsContainerName = null
    ) =>
        new(
            CreateAzureBlobStorageClientFactory(),
            new NullLogger<AzureBlobStorageHealthCheckStrategy>(),
            Microsoft.Extensions.Options.Options.Create(
                new AppOptions
                {
                    SearchStorageConnectionString =
                        searchStorageConnectionString ?? IntegrationTestStorageAccountConnectionString,
                    SearchableDocumentsContainerName = searchableDocumentsContainerName ?? IntegrationTestContainerName,
                }
            )
        );

    private static Func<IAzureBlobStorageClient> CreateAzureBlobStorageClientFactory() =>
        () =>
            new AzureBlobStorageClient(
                new BlobServiceClient(IntegrationTestStorageAccountConnectionString),
                new NullLogger<AzureBlobStorageClient>()
            );

    [Fact(Skip = "Complete StorageAccountName and StorageAccountAccessKey to run this integration test")]
    public async Task WhenCorrectlyConfigured_ThenShouldRespondIsHealthy()
    {
        //  ARRANGE
        var sut = GetSUT();

        // ACT
        var healthCheckResult = await sut.Run(CancellationToken.None);

        // ASSERT
        Assert.NotNull(healthCheckResult);
        Assert.True(healthCheckResult.IsHealthy);
    }

    [Fact(Skip = "Complete StorageAccountName and StorageAccountAccessKey to run this integration test")]
    public async Task WhenContainerNotFound_ThenShouldRespondIsUnhealthy()
    {
        //  ARRANGE
        var sut = GetSUT(searchableDocumentsContainerName: "some-unknown-container-name");

        // ACT
        var healthCheckResult = await sut.Run(CancellationToken.None);

        // ASSERT
        Assert.NotNull(healthCheckResult);
        Assert.False(healthCheckResult.IsHealthy);
        Assert.Contains("container", healthCheckResult.Message);
        Assert.Contains("not found", healthCheckResult.Message);
    }

    [Fact]
    public async Task WhenContainerNameIsMissingFromConfig_ThenShouldRespondAsUnhealthy()
    {
        //  ARRANGE
        // Missing container name
        var sut = GetSUT(searchableDocumentsContainerName: string.Empty);

        // ACT
        var healthCheckResult = await sut.Run(CancellationToken.None);

        // ASSERT
        Assert.NotNull(healthCheckResult);
        Assert.False(healthCheckResult.IsHealthy);
        Assert.Contains("Container", healthCheckResult.Message);
        Assert.Contains("config", healthCheckResult.Message);
    }

    [Fact]
    public async Task WhenConnectionStringIsMissingFromConfig_ThenShouldRespondAsUnhealthy()
    {
        //  ARRANGE
        var sut = GetSUT(searchStorageConnectionString: string.Empty);

        // ACT
        var healthCheckResult = await sut.Run(CancellationToken.None);

        // ASSERT
        Assert.NotNull(healthCheckResult);
        Assert.False(healthCheckResult.IsHealthy);
        Assert.Contains("Connection String", healthCheckResult.Message);
        Assert.Contains("config", healthCheckResult.Message);
    }
}
