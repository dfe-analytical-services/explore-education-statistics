using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.RemoveReleaseDocuments;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Builders;

public class ReleaseDocumentRemoverMockBuilder
{
    private readonly Mock<IReleaseDocumentRemover> _mock = new(MockBehavior.Strict);
    private RemovePublicationReleaseDocumentsResponse? _removePublicationReleaseDocumentsResponse;
    private RemoveReleaseDocumentResponse? _removeReleaseDocumentResponse;

    public ReleaseDocumentRemoverMockBuilder()
    {
        Assert = new Asserter(_mock);

        _mock
            .Setup(m =>
                m.RemovePublicationReleaseDocuments(
                    It.IsAny<RemovePublicationReleaseDocumentsRequest>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(
                _removePublicationReleaseDocumentsResponse
                    ?? new RemovePublicationReleaseDocumentsResponse(new Dictionary<Guid, bool>())
            );

        _mock
            .Setup(m =>
                m.RemoveReleaseDocument(It.IsAny<RemoveReleaseDocumentRequest>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(_removeReleaseDocumentResponse ?? new RemoveReleaseDocumentResponse(Success: true));

        _mock.Setup(m => m.RemoveAllReleaseDocuments(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
    }

    public IReleaseDocumentRemover Build()
    {
        return _mock.Object;
    }

    public ReleaseDocumentRemoverMockBuilder WhereResponseIs(RemovePublicationReleaseDocumentsResponse response)
    {
        _removePublicationReleaseDocumentsResponse = response;
        return this;
    }

    public ReleaseDocumentRemoverMockBuilder WhereResponseIs(RemoveReleaseDocumentResponse response)
    {
        _removeReleaseDocumentResponse = response;
        return this;
    }

    public Asserter Assert { get; }

    public class Asserter(Mock<IReleaseDocumentRemover> mock)
    {
        public void RemovePublicationReleaseDocumentsCalledFor(string publicationSlug)
        {
            mock.Verify(
                m =>
                    m.RemovePublicationReleaseDocuments(
                        It.Is<RemovePublicationReleaseDocumentsRequest>(req => req.PublicationSlug == publicationSlug),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once
            );
        }

        public void RemovePublicationReleaseDocumentsNotCalled()
        {
            mock.Verify(
                m =>
                    m.RemovePublicationReleaseDocuments(
                        It.IsAny<RemovePublicationReleaseDocumentsRequest>(),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Never
            );
        }

        public void RemoveReleaseDocumentCalledFor(Guid releaseId)
        {
            mock.Verify(
                m =>
                    m.RemoveReleaseDocument(
                        It.Is<RemoveReleaseDocumentRequest>(req => req.ReleaseId == releaseId),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once
            );
        }

        public void RemoveReleaseDocumentNotCalled()
        {
            mock.Verify(
                m => m.RemoveReleaseDocument(It.IsAny<RemoveReleaseDocumentRequest>(), It.IsAny<CancellationToken>()),
                Times.Never
            );
        }

        public void AllReleaseDocumentsRemoved()
        {
            mock.Verify(m => m.RemoveAllReleaseDocuments(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
