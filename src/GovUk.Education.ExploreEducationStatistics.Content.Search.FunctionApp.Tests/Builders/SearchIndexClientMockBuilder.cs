using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.AzureSearch;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Builders;

public class SearchIndexClientMockBuilder
{
    private readonly Mock<ISearchIndexerClient> _mock = new(MockBehavior.Strict);
    private readonly List<string> _indexersNotFound = [];
    private string? _indexerExistsThrowsFor;

    public ISearchIndexerClient Build()
    {
        _mock.Setup(m => m.RunIndexer(It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        _mock
            .Setup(m => m.IndexerExists(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string indexerName, CancellationToken _) => !_indexersNotFound.Contains(indexerName));

        if (_indexerExistsThrowsFor is not null)
        {
            _mock
                .Setup(m => m.IndexerExists(_indexerExistsThrowsFor, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Indexer exists check failed"));
        }

        return _mock.Object;
    }

    public SearchIndexClientMockBuilder WhereIndexerIsNotFound(string indexerName)
    {
        _indexersNotFound.Add(indexerName);
        return this;
    }

    public SearchIndexClientMockBuilder WhereIndexerExistsThrowsFor(string indexerName)
    {
        _indexerExistsThrowsFor = indexerName;
        return this;
    }

    public Asserter Assert => new Asserter(_mock);

    public class Asserter(Mock<ISearchIndexerClient> mock)
    {
        public void IndexerRun(string indexerName) =>
            mock.Verify(m => m.RunIndexer(indexerName, It.IsAny<CancellationToken>()), Times.Once);
    }
}
