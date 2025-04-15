using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.ContentApi;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Domain;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Builders;

internal class ContentApiClientMockBuilder
{
    private readonly Mock<IContentApiClient> _mock = new(MockBehavior.Strict);
    private readonly ReleaseSearchableDocumentBuilder _releaseSearchableDocumentBuilder = new();
    private ReleaseSearchableDocument? _releaseSearchableDocument;
    private PublicationInfo[]? _publicationsForTheme;

    public ContentApiClientMockBuilder()
    {
        Assert = new Asserter(_mock);
        
        _mock
            .Setup(m => m.GetPublicationLatestReleaseSearchableDocument(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_releaseSearchableDocument ?? _releaseSearchableDocumentBuilder.Build());

        _mock
            .Setup(m => m.GetPublicationsForTheme(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => _publicationsForTheme ?? []);
    }

    public IContentApiClient Build()
    {
        return _mock.Object;
    }

    public ContentApiClientMockBuilder WhereReleaseSearchViewModelIs(ReleaseSearchableDocument releaseSearchableDocument)
    {
        _releaseSearchableDocument = releaseSearchableDocument;
        return this;
    }
    
    public ContentApiClientMockBuilder WhereReleaseSearchViewModelIs(Func<ReleaseSearchableDocumentBuilder, ReleaseSearchableDocumentBuilder> modifyReleaseSearchViewModel)
    {
        modifyReleaseSearchViewModel(_releaseSearchableDocumentBuilder);
        return this;
    }

    public ContentApiClientMockBuilder WhereThemeHasPublications(params PublicationInfo[] publications)
    {
        _publicationsForTheme = publications;
        return this;
    }

    public Asserter Assert { get; }
    public class Asserter(Mock<IContentApiClient> mock)
    {
        public void ContentWasLoadedFor(string publicationSlug)
        {
            mock.Verify(m => m.GetPublicationLatestReleaseSearchableDocument(publicationSlug, It.IsAny<CancellationToken>()), Times.Once);
        }

        public void PublicationsRequestedForThemeId(Guid expectedThemeId)
        {
            mock.Verify(m => m.GetPublicationsForTheme(expectedThemeId, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
