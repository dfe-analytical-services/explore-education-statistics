using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Domain;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.CheckSearchableDocuments;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Services.CheckSearchableDocuments;

public class ReleaseSummaryRetrieverMockBuilder
{
    private readonly Mock<IReleaseSummaryRetriever> _mock = new(MockBehavior.Strict);
    private IList<ReleaseSummary> _releaseSummaries = [];

    public IReleaseSummaryRetriever Build()
    {
        _mock
            .Setup(m => m.GetAllPublishedReleaseSummaries(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_releaseSummaries);

        return _mock.Object;
    }

    public ReleaseSummaryRetrieverMockBuilder WhereReleaseSummariesReturnedAre(IList<ReleaseSummary> releaseSummaries)
    {
        _releaseSummaries = releaseSummaries;
        return this;
    }
}
