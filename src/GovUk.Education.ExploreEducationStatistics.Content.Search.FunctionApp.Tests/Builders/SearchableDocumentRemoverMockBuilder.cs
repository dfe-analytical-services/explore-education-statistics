using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Builders;

public class SearchableDocumentRemoverMockBuilder
{
    private readonly Mock<ISearchableDocumentRemover> _mock = new(MockBehavior.Strict);
    private RemovePublicationSearchableDocumentsResponse? _response;

    public SearchableDocumentRemoverMockBuilder()
    {
        Assert = new Asserter(_mock);

        _mock
            .Setup(m => m.RemovePublicationSearchableDocuments(
                It.IsAny<RemovePublicationSearchableDocumentsRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(_response ?? new RemovePublicationSearchableDocumentsResponse(new Dictionary<Guid, bool>()));
    }

    public ISearchableDocumentRemover Build()
    {
        return _mock.Object;
    }

    public SearchableDocumentRemoverMockBuilder WhereResponseIs(RemovePublicationSearchableDocumentsResponse response)
    {
        _response = response;
        return this;
    }

    public Asserter Assert { get; }

    public class Asserter(Mock<ISearchableDocumentRemover> mock)
    {
        public void RemovePublicationSearchableDocumentsCalledFor(string publicationSlug)
        {
            mock
                .Verify(m => m.RemovePublicationSearchableDocuments(
                        It.Is<RemovePublicationSearchableDocumentsRequest>(
                            req =>
                                req.PublicationSlug == publicationSlug),
                        It.IsAny<CancellationToken>()),
                    Times.Once);
        }

        public void RemovePublicationSearchableDocumentsNotCalled()
        {
            mock.Verify(m => m.RemovePublicationSearchableDocuments(
                    It.IsAny<RemovePublicationSearchableDocumentsRequest>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
