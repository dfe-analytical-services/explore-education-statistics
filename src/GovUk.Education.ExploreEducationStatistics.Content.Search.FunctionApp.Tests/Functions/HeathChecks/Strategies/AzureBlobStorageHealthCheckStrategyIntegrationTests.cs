using Azure.Storage.Blobs;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.AzureBlobStorage;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.HealthChecks.Strategies;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Options;
using Microsoft.Extensions.Logging;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Functions.HeathChecks.Strategies;

public class AzureBlobStorageHealthCheckStrategyIntegrationTests
{
    private const string StorageAccountName = "-- azure storage account name here --";
    private const string StorageAccountAccessKey = "-- azure storage account access key here --";
            
    private const string IntegrationTestStorageAccountConnectionString = $"AccountName={StorageAccountName};AccountKey={StorageAccountAccessKey};";
    private const string IntegrationTestContainerName = "integration-tests";


    [Fact(Skip = "Complete StorageAccountName and StorageAccountAccessKey to run this intergration test")]
    public async Task WhenCorrectlyConfigured_ThenShouldRespondIsHealthy()
    {
        //  ARRANGE
        var sut = new AzureBlobStorageHealthCheckStrategy(
            new AzureBlobStorageClient(
                new BlobServiceClient(IntegrationTestStorageAccountConnectionString),
                Mock.Of<ILogger<AzureBlobStorageClient>>()),
            Microsoft.Extensions.Options.Options.Create(new AppOptions
            {
                SearchStorageConnectionString = IntegrationTestStorageAccountConnectionString,
                SearchableDocumentsContainerName = IntegrationTestContainerName
            }));

        // ACT
        var heathCheckResult = await sut.Run(CancellationToken.None);
        
        // ASSERT
        Assert.NotNull(heathCheckResult);
        Assert.True(heathCheckResult.IsHealthy);
    }

    [Fact(Skip = "Complete StorageAccountName and StorageAccountAccessKey to run this intergration test")]
    public async Task WhenContainerNotConfigured_ThenShouldRespondIsUnhealthy()
    {
        //  ARRANGE
        var sut = new AzureBlobStorageHealthCheckStrategy(
            new AzureBlobStorageClient(
                new BlobServiceClient(IntegrationTestStorageAccountConnectionString),
                Mock.Of<ILogger<AzureBlobStorageClient>>()),
            Microsoft.Extensions.Options.Options.Create(new AppOptions
            {
                SearchStorageConnectionString = IntegrationTestStorageAccountConnectionString,
                SearchableDocumentsContainerName = string.Empty // No container name
            }));

        // ACT
        var heathCheckResult = await sut.Run(CancellationToken.None);
        
        // ASSERT
        Assert.NotNull(heathCheckResult);
        Assert.False(heathCheckResult.IsHealthy);
    }
    
    [Fact(Skip = "Complete StorageAccountName and StorageAccountAccessKey to run this intergration test")]
    public async Task WhenContainerNotFound_ThenShouldRespondIsUnhealthy()
    {
        //  ARRANGE
        var sut = new AzureBlobStorageHealthCheckStrategy(
            new AzureBlobStorageClient(
                new BlobServiceClient(IntegrationTestStorageAccountConnectionString),
                Mock.Of<ILogger<AzureBlobStorageClient>>()),
            Microsoft.Extensions.Options.Options.Create(new AppOptions
            {
                SearchStorageConnectionString = IntegrationTestStorageAccountConnectionString,
                SearchableDocumentsContainerName = "some-unknown-container-name"
            }));

        // ACT
        var heathCheckResult = await sut.Run(CancellationToken.None);
        
        // ASSERT
        Assert.NotNull(heathCheckResult);
        Assert.False(heathCheckResult.IsHealthy);
        Assert.Contains("container", heathCheckResult.Message);
        Assert.Contains("not found", heathCheckResult.Message);
    }
}
