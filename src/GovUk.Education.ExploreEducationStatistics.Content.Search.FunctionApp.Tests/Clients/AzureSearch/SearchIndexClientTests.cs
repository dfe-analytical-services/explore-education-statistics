using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.AzureSearch;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Options;
using SearchIndexClient = GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.AzureSearch.SearchIndexClient;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Clients.AzureSearch;

public class SearchIndexClientTests
{
    private AzureSearchOptions AzureSearchOptions => new()
    {
        SearchServiceEndpoint = _searchServiceEndpoint,
        SearchServiceAccessKey = _searchServiceAccessKey,
        IndexerName = _indexerName
    };


    private string _searchServiceEndpoint = "https://example.search.windows.net/";
    private string? _searchServiceAccessKey = "my-access-key";
    private string _indexerName = "my-indexer-name";

    private ISearchIndexClient GetSut()
    {
        return new SearchIndexClient(
            new AzureSearchIndexerClientFactory(
                Microsoft.Extensions.Options.Options.Create(AzureSearchOptions)),
            Microsoft.Extensions.Options.Options.Create(AzureSearchOptions));
    }

    public class BasicTests : SearchIndexClientTests
    {
        [Fact]
        public void Can_Instantiate_SUT() => Assert.NotNull(GetSut());
    }

    public class IntegrationTests : SearchIndexClientTests
    {
        public IntegrationTests()
        {
            var searchServiceName = "-- insert search service name here --";
            _searchServiceAccessKey = "-- insert search service access key here --";
            _indexerName = "-- insert search indexer name here --";

            _searchServiceEndpoint = $"https://{searchServiceName}.search.windows.net/";
        }
        
        [Fact(Skip = "Integration test is disabled")]
        public async Task Trigger_reindex()
        {
            // ARRANGE
            var sut = GetSut();

            // ACT
            await sut.RunIndexer();
        }
        
        [Fact(Skip = "Integration test is disabled")]
        public async Task Can_ping_indexer()
        {
            // ARRANGE
            var sut = GetSut();

            // ACT
            Assert.True(await sut.IndexerExists());
        }
        
        [Fact(Skip = "Integration test is disabled")]
        public async Task Can_not_ping_unknown_indexer()
        {
            // ARRANGE
            _indexerName = "not-a-real-indexer";
            var sut = GetSut();

            // ACT
            Assert.False(await sut.IndexerExists());
        }
    }
}
