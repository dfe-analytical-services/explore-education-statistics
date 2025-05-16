using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.RemoveSearchableDocument;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Builders;

public class SearchableDocumentRemoverMockBuilder
{
    private readonly Mock<ISearchableDocumentRemover> _mock = new(MockBehavior.Strict);
    private RemovePublicationSearchableDocumentsResponse? _removePublicationSearchableDocumentsResponse;
    private RemoveSearchableDocumentResponse? _removeSearchableDocumentResponse;

    public SearchableDocumentRemoverMockBuilder()
    {
        Assert = new Asserter(_mock);

        _mock
            .Setup(m => m.RemovePublicationSearchableDocuments(
                It.IsAny<RemovePublicationSearchableDocumentsRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(_removePublicationSearchableDocumentsResponse ?? new RemovePublicationSearchableDocumentsResponse(new Dictionary<Guid, bool>()));
        
        _mock
            .Setup(m => m.RemoveSearchableDocument(
                It.IsAny<RemoveSearchableDocumentRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(_removeSearchableDocumentResponse ?? new RemoveSearchableDocumentResponse(Success: true));
        
        _mock
            .Setup(m => m.RemoveAllSearchableDocuments(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    public ISearchableDocumentRemover Build()
    {
        return _mock.Object;
    }

    public SearchableDocumentRemoverMockBuilder WhereResponseIs(RemovePublicationSearchableDocumentsResponse response)
    {
        _removePublicationSearchableDocumentsResponse = response;
        return this;
    }

    public SearchableDocumentRemoverMockBuilder WhereResponseIs(RemoveSearchableDocumentResponse response)
    {
        _removeSearchableDocumentResponse = response;
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

        public void RemoveSearchableDocumentCalledFor(Guid releaseId)
        {
            mock
                .Verify(m => m.RemoveSearchableDocument(
                        It.Is<RemoveSearchableDocumentRequest>(
                            req =>
                                req.ReleaseId == releaseId),
                        It.IsAny<CancellationToken>()),
                    Times.Once);
        }

        public void RemoveSearchableDocumentNotCalled()
        {
            mock
                .Verify(m => m.RemoveSearchableDocument(
                        It.IsAny<RemoveSearchableDocumentRequest>(),
                        It.IsAny<CancellationToken>()),
                    Times.Never);
        }

        public void AllSearchableDocumentsRemoved()
        {
            mock.Verify(m => m.RemoveAllSearchableDocuments(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
