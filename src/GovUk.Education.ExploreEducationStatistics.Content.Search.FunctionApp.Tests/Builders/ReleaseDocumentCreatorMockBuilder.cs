using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.CreateReleaseDocuments;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Builders;

public class ReleaseDocumentCreatorMockBuilder
{
    private readonly Mock<IReleaseDocumentCreator> _mock = new(MockBehavior.Strict);
    private CreatePublicationLatestReleaseDocumentResponse? _response;

    public IReleaseDocumentCreator Build()
    {
        _mock
            .Setup(m =>
                m.CreatePublicationLatestReleaseDocument(
                    It.IsAny<CreatePublicationLatestReleaseDocumentRequest>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(
                (CreatePublicationLatestReleaseDocumentRequest req, CancellationToken _) =>
                    _response
                    ?? new CreatePublicationLatestReleaseDocumentResponse
                    {
                        PublicationSlug = req.PublicationSlug,
                        ReleaseId = Guid.NewGuid(),
                        ReleaseSlug = "release-slug",
                        ReleaseVersionId = Guid.NewGuid(),
                        BlobName = "blob name",
                    }
            );

        return _mock.Object;
    }

    public ReleaseDocumentCreatorMockBuilder WhereResponseIs(CreatePublicationLatestReleaseDocumentResponse response)
    {
        _response = response;
        return this;
    }

    public Asserter Assert => new(_mock);

    public class Asserter(Mock<IReleaseDocumentCreator> mock)
    {
        public void CreateReleaseDocumentCalledFor(string expectedPublicationSlug)
        {
            mock.Verify(
                m =>
                    m.CreatePublicationLatestReleaseDocument(
                        It.Is<CreatePublicationLatestReleaseDocumentRequest>(req =>
                            req.PublicationSlug == expectedPublicationSlug
                        ),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once
            );
        }

        public void CreateReleaseDocumentNotCalled()
        {
            mock.Verify(
                m =>
                    m.CreatePublicationLatestReleaseDocument(
                        It.IsAny<CreatePublicationLatestReleaseDocumentRequest>(),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Never
            );
        }
    }
}
