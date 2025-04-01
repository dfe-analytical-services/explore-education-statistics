using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.ContentApi;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Domain;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Builders;

internal class ContentApiClientBuilder
{
    private readonly Mock<IContentApiClient> _mock = new Mock<IContentApiClient>(MockBehavior.Strict);
    private readonly ReleaseSearchableDocumentBuilder _releaseSearchableDocumentBuilder = new();
    private ReleaseSearchableDocument? _releaseSearchableDocument;

    public ContentApiClientBuilder()
    {
        Assert = new Asserter(_mock);
        
        _mock
            .Setup(m => m.GetPublicationLatestReleaseSearchableDocument(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_releaseSearchableDocument ?? _releaseSearchableDocumentBuilder.Build());
    }

    public IContentApiClient Build()
    {
        return _mock.Object;
    }

    public ContentApiClientBuilder WhereReleaseSearchViewModelIs(ReleaseSearchableDocument releaseSearchableDocument)
    {
        _releaseSearchableDocument = releaseSearchableDocument;
        return this;
    }
    
    public ContentApiClientBuilder WhereReleaseSearchViewModelIs(Func<ReleaseSearchableDocumentBuilder, ReleaseSearchableDocumentBuilder> modifyReleaseSearchViewModel)
    {
        modifyReleaseSearchViewModel(_releaseSearchableDocumentBuilder);
        return this;
    }

    public Asserter Assert { get; }
    public class Asserter(Mock<IContentApiClient> mock)
    {
        public void ContentWasLoadedFor(string publicationSlug)
        {
            mock.Verify(m => m.GetPublicationLatestReleaseSearchableDocument(publicationSlug, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
