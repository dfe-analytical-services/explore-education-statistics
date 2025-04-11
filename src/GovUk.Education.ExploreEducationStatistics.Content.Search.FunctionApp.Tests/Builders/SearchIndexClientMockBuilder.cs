using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.AzureSearch;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Builders;

public class SearchIndexClientMockBuilder
{
    private readonly Mock<ISearchIndexClient> _mock = new(MockBehavior.Strict);
    public ISearchIndexClient Build()
    {
        _mock
            .Setup(m => m.RunIndexer(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        
        return _mock.Object;
    }

    public Asserter Assert => new Asserter(_mock);
    public class Asserter(Mock<ISearchIndexClient> mock)
    {
        public void IndexerRun() => mock.Verify(m => m.RunIndexer(It.IsAny<CancellationToken>()), Times.Once);
    }
}
