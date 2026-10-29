using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.HealthChecks.Strategies;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Options;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Builders;
using Microsoft.Extensions.Logging.Abstractions;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Functions.HealthChecks.Strategies;

public class AzureBlobStorageHealthCheckStrategyTests
{
    private const string ReleaseDocumentsContainerName = "release-documents-container-name";
    private const string DatasetDocumentsContainerName = "dataset-documents-container-name";

    private readonly AzureBlobStorageClientMockBuilder _azureBlobStorageClientMockBuilder = new();

    private AzureBlobStorageHealthCheckStrategy GetSut() =>
        new(
            () => _azureBlobStorageClientMockBuilder.Build(),
            new NullLogger<AzureBlobStorageHealthCheckStrategy>(),
            Microsoft.Extensions.Options.Options.Create(
                new AppOptions
                {
                    SearchStorageConnectionString = "UseDevelopmentStorage=true",
                    ReleaseDocumentsContainerName = ReleaseDocumentsContainerName,
                    DatasetDocumentsContainerName = DatasetDocumentsContainerName,
                }
            )
        );

    [Fact]
    public void Can_instantiate_SUT() => Assert.NotNull(GetSut());

    [Fact]
    public async Task WhenAllContainersFound_ThenShouldRespondIsHealthy()
    {
        // ARRANGE
        var sut = GetSut();

        // ACT
        var healthCheckResult = await sut.Run(CancellationToken.None);

        // ASSERT
        Assert.True(healthCheckResult.IsHealthy);
        Assert.Contains(
            $"Connection to Release Documents Azure Blob Storage container '{ReleaseDocumentsContainerName}':OK.",
            healthCheckResult.Message
        );
        Assert.Contains(
            $"Connection to Dataset Documents Azure Blob Storage container '{DatasetDocumentsContainerName}':OK.",
            healthCheckResult.Message
        );
    }

    [Theory]
    [InlineData(ReleaseDocumentsContainerName, "Release Documents")]
    [InlineData(DatasetDocumentsContainerName, "Dataset Documents")]
    public async Task WhenContainerNotFound_ThenShouldRespondIsUnhealthy(string containerName, string description)
    {
        // ARRANGE
        _azureBlobStorageClientMockBuilder.WhereContainerIsNotFound(containerName);
        var sut = GetSut();

        // ACT
        var healthCheckResult = await sut.Run(CancellationToken.None);

        // ASSERT
        Assert.False(healthCheckResult.IsHealthy);
        Assert.Contains(
            $"{description} Azure Blob Storage container '{containerName}' is not found.",
            healthCheckResult.Message
        );
    }

    [Fact]
    public async Task WhenContainerNotFound_ThenShouldStillReportOtherContainers()
    {
        // ARRANGE
        _azureBlobStorageClientMockBuilder.WhereContainerIsNotFound(ReleaseDocumentsContainerName);
        var sut = GetSut();

        // ACT
        var healthCheckResult = await sut.Run(CancellationToken.None);

        // ASSERT
        Assert.False(healthCheckResult.IsHealthy);
        Assert.Contains(
            $"Connection to Dataset Documents Azure Blob Storage container '{DatasetDocumentsContainerName}':OK.",
            healthCheckResult.Message
        );
    }

    [Fact]
    public async Task WhenContainerCheckThrows_ThenShouldRespondIsUnhealthy()
    {
        // ARRANGE
        _azureBlobStorageClientMockBuilder.WhereContainerExistsThrowsFor(DatasetDocumentsContainerName);
        var sut = GetSut();

        // ACT
        var healthCheckResult = await sut.Run(CancellationToken.None);

        // ASSERT
        Assert.False(healthCheckResult.IsHealthy);
        Assert.Contains(
            $"Error occurred whilst trying to check for Dataset Documents Azure Blob Storage container '{DatasetDocumentsContainerName}': Container exists check failed",
            healthCheckResult.Message
        );
    }
}
