using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Domain;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.CheckSearchableDocuments;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Services.CheckSearchableDocuments;

public class ReleaseVersionSummaryRetrieverMockBuilder
{
    private readonly Mock<IReleaseVersionSummaryRetriever> _mock = new(MockBehavior.Strict);
    private IList<ReleaseVersionSummary> _releaseVersionSummaries = [];

    public IReleaseVersionSummaryRetriever Build()
    {
        _mock
            .Setup(m => m.GetAllPublishedReleaseVersionSummaries(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_releaseVersionSummaries);

        return _mock.Object;
    }

    public ReleaseVersionSummaryRetrieverMockBuilder WhereReleaseVersionSummariesReturnedAre(
        IList<ReleaseVersionSummary> releaseVersionSummaries
    )
    {
        _releaseVersionSummaries = releaseVersionSummaries;
        return this;
    }
}
