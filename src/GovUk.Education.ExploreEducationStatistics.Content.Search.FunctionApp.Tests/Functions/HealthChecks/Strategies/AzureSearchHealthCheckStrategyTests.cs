using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.HealthChecks.Strategies;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Options;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Builders;
using Microsoft.Extensions.Logging.Abstractions;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Functions.HealthChecks.Strategies;

public class AzureSearchHealthCheckStrategyTests
{
    private const string ReleaseIndexerName = "release-indexer-name";
    private const string DatasetIndexerName = "dataset-indexer-name";

    private readonly SearchIndexClientMockBuilder _searchIndexClientMockBuilder = new();

    private AzureSearchOptions _azureSearchOptions = new()
    {
        SearchServiceEndpoint = "https://example.search.windows.net/",
        ReleaseIndexerName = ReleaseIndexerName,
        DatasetIndexerName = DatasetIndexerName,
    };

    private AzureSearchHealthCheckStrategy GetSut() =>
        new(
            () => _searchIndexClientMockBuilder.Build(),
            new NullLogger<AzureSearchHealthCheckStrategy>(),
            Microsoft.Extensions.Options.Options.Create(_azureSearchOptions)
        );

    [Fact]
    public void Can_instantiate_SUT() => Assert.NotNull(GetSut());

    [Fact]
    public async Task WhenAllIndexersFound_ThenShouldRespondIsHealthy()
    {
        // ARRANGE
        var sut = GetSut();

        // ACT
        var healthCheckResult = await sut.Run(CancellationToken.None);

        // ASSERT
        Assert.True(healthCheckResult.IsHealthy);
        Assert.Contains(
            $"Connection to Release Azure Search Indexer '{ReleaseIndexerName}':OK.",
            healthCheckResult.Message
        );
        Assert.Contains(
            $"Connection to Dataset Azure Search Indexer '{DatasetIndexerName}':OK.",
            healthCheckResult.Message
        );
    }

    [Theory]
    [InlineData(ReleaseIndexerName, "Release")]
    [InlineData(DatasetIndexerName, "Dataset")]
    public async Task WhenIndexerNotFound_ThenShouldRespondIsUnhealthy(string indexerName, string description)
    {
        // ARRANGE
        _searchIndexClientMockBuilder.WhereIndexerIsNotFound(indexerName);
        var sut = GetSut();

        // ACT
        var healthCheckResult = await sut.Run(CancellationToken.None);

        // ASSERT
        Assert.False(healthCheckResult.IsHealthy);
        Assert.Contains($"{description} Azure Search Indexer '{indexerName}' is not found.", healthCheckResult.Message);
    }

    [Fact]
    public async Task WhenIndexerNotFound_ThenShouldStillReportOtherIndexers()
    {
        // ARRANGE
        _searchIndexClientMockBuilder.WhereIndexerIsNotFound(ReleaseIndexerName);
        var sut = GetSut();

        // ACT
        var healthCheckResult = await sut.Run(CancellationToken.None);

        // ASSERT
        Assert.False(healthCheckResult.IsHealthy);
        Assert.Contains(
            $"Connection to Dataset Azure Search Indexer '{DatasetIndexerName}':OK.",
            healthCheckResult.Message
        );
    }

    [Fact]
    public async Task WhenIndexerCheckThrows_ThenShouldRespondIsUnhealthy()
    {
        // ARRANGE
        _searchIndexClientMockBuilder.WhereIndexerExistsThrowsFor(DatasetIndexerName);
        var sut = GetSut();

        // ACT
        var healthCheckResult = await sut.Run(CancellationToken.None);

        // ASSERT
        Assert.False(healthCheckResult.IsHealthy);
        Assert.Contains(
            $"Error occurred whilst trying to run health check for Dataset Azure Search Indexer '{DatasetIndexerName}': Indexer exists check failed",
            healthCheckResult.Message
        );
    }

    [Fact]
    public async Task WhenDatasetIndexerNameIsMissingFromConfig_ThenShouldRespondAsUnhealthy()
    {
        // ARRANGE
        _azureSearchOptions = new AzureSearchOptions
        {
            SearchServiceEndpoint = _azureSearchOptions.SearchServiceEndpoint,
            ReleaseIndexerName = ReleaseIndexerName,
        };
        var sut = GetSut();

        // ACT
        var healthCheckResult = await sut.Run(CancellationToken.None);

        // ASSERT
        Assert.False(healthCheckResult.IsHealthy);
        Assert.Contains("Azure Search Dataset Indexer Name is not configured.", healthCheckResult.Message);
    }
}
