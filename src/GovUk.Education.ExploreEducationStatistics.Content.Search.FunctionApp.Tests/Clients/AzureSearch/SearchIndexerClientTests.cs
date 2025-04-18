using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.AzureSearch;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Options;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Builders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit.Abstractions;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Clients.AzureSearch;

public abstract class SearchIndexerClientTests
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

    protected virtual ISearchIndexerClient GetSut(ILogger<SearchIndexerClient>? logger = null)
    {
        return new SearchIndexerClient(
            new AzureSearchIndexerClientFactory(
                Microsoft.Extensions.Options.Options.Create(AzureSearchOptions)),
            Microsoft.Extensions.Options.Options.Create(AzureSearchOptions),
            logger ?? new NullLogger<SearchIndexerClient>());
    }

    public class BasicTests : SearchIndexerClientTests
    {
        [Fact]
        public void Can_Instantiate_SUT() => Assert.NotNull(GetSut());
    }

    public class IntegrationTests : SearchIndexerClientTests
    {
        private readonly ITestOutputHelper _output;

        public IntegrationTests(ITestOutputHelper output)
        {
            _output = output;

            var searchServiceName = "-- insert search service name here --";
            _searchServiceAccessKey = "-- insert search service access key here --";
            _indexerName = "-- insert search indexer name here --";

            _searchServiceEndpoint = $"https://{searchServiceName}.search.windows.net/";
        }

        protected override ISearchIndexerClient GetSut(ILogger<SearchIndexerClient>? logger = null) =>
            base.GetSut(new LoggerMockBuilder<SearchIndexerClient>()
                .WithLogAction(s => _output.WriteLine(s))
                .Build());

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

        [Fact(Skip = "Integration test is disabled")]
        public async Task Run_indexer()
        {
            var sut = GetSut();
            await sut.RunIndexer();
        }
        
        [Fact(Skip = "Integration test is disabled")]
        public async Task Reset_indexer()
        {
            var sut = GetSut();
            await sut.ResetIndexer();
        }

        [Fact(Skip = "Integration test is disabled")]
        public async Task Run_indexer_multiple_times()
        {
            var sut = GetSut();
            var tasks = new List<Task>();
            for (var i = 0; i < 10; i++)
            {
                tasks.Add(sut.RunIndexer());
                await Task.Delay(TimeSpan.FromMilliseconds(50));
            }
            await Task.WhenAll(tasks);
        }

        [Fact(Skip = "Integration test is disabled")]
        public async Task Is_indexer_running()
        {
            var sut = GetSut();
            var isIndexerRunning = await sut.IsIndexerRunning(_indexerName);
            Print($"Is indexer {_indexerName} running: {isIndexerRunning}");
        }
        
        private void Print(string message) => _output.WriteLine(message);
    }
}
