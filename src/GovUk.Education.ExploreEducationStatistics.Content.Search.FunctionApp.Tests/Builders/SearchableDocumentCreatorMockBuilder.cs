using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.CreateSearchableDocuments;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Builders;

public class SearchableDocumentCreatorMockBuilder
{
    private readonly Mock<ISearchableDocumentCreator> _mock = new(MockBehavior.Strict);
    private CreatePublicationLatestReleaseSearchableDocumentResponse? _response;

    public ISearchableDocumentCreator Build()
    {
        _mock
            .Setup(
                m =>
                    m.CreatePublicationLatestReleaseSearchableDocument(
                        It.IsAny<CreatePublicationLatestReleaseSearchableDocumentRequest>(),
                        It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                (CreatePublicationLatestReleaseSearchableDocumentRequest req, CancellationToken _) =>
                    _response ??
                    new CreatePublicationLatestReleaseSearchableDocumentResponse
                    {
                        PublicationSlug = req.PublicationSlug,
                        ReleaseId = Guid.NewGuid(),
                        ReleaseSlug = "release-slug",
                        ReleaseVersionId = Guid.NewGuid(),
                        BlobName = "blob name"
                    });

        return _mock.Object;
    }

    public SearchableDocumentCreatorMockBuilder WhereResponseIs(
        CreatePublicationLatestReleaseSearchableDocumentResponse response)
    {
        _response = response;
        return this;
    }

public Asserter Assert => new(_mock);

    public class Asserter(Mock<ISearchableDocumentCreator> mock)
    {
        public void CreateSearchableDocumentCalledFor(string expectedPublicationSlug)
        {
            mock
                .Verify(
                    m =>
                        m.CreatePublicationLatestReleaseSearchableDocument(
                            It.Is<CreatePublicationLatestReleaseSearchableDocumentRequest>(
                                req => 
                                    req.PublicationSlug == expectedPublicationSlug),
                    It.IsAny<CancellationToken>()),
                    Times.Once);
        }

        public void CreateSearchableDocumentNotCalled()
        {
            mock
                .Verify(
                    m =>
                        m.CreatePublicationLatestReleaseSearchableDocument(
                            It.IsAny<CreatePublicationLatestReleaseSearchableDocumentRequest>(),
                            It.IsAny<CancellationToken>()),
                    Times.Never);
        }
    }
}
