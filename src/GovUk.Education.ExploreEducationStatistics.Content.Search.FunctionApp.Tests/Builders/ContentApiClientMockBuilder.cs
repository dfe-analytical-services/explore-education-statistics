using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.ContentApi;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Domain;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Exceptions;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Builders;

internal class ContentApiClientMockBuilder
{
    private readonly Mock<IContentApiClient> _mock = new(MockBehavior.Strict);
    private readonly ReleaseSearchableDocumentBuilder _releaseSearchableDocumentBuilder = new();
    private ReleaseSearchableDocument? _releaseSearchableDocument;
    private PublicationInfo[]? _publicationsForTheme;
    private ReleaseInfo[]? _releasesForPublication;
    private PublicationInfo[]? _publications;

    public ContentApiClientMockBuilder()
    {
        Assert = new Asserter(_mock);

        _mock
            .Setup(m =>
                m.GetPublicationLatestReleaseSearchableDocument(It.IsAny<string>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(() => _releaseSearchableDocument ?? _releaseSearchableDocumentBuilder.Build());

        _mock
            .Setup(m => m.GetPublicationsForTheme(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => _publicationsForTheme ?? []);

        _mock
            .Setup(m => m.GetReleasesForPublication(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => _releasesForPublication ?? []);

        _mock
            .Setup(m => m.GetAllLivePublicationInfos(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => _publications ?? []);

        _mock
            .Setup(m => m.GetReleaseSummary(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                (string publicationSlug, string releaseSlug, CancellationToken _) =>
                    new ReleaseSummary
                    {
                        Id = Guid.NewGuid().ToString(),
                        ReleaseId = Guid.NewGuid().ToString(),
                        Title = "Release title",
                        Slug = releaseSlug,
                        PublicationSlug = publicationSlug,
                    }
            );
    }

    public IContentApiClient Build()
    {
        return _mock.Object;
    }

    public ContentApiClientMockBuilder WhereReleaseSearchViewModelIs(
        ReleaseSearchableDocument releaseSearchableDocument
    )
    {
        _releaseSearchableDocument = releaseSearchableDocument;
        return this;
    }

    public ContentApiClientMockBuilder WhereReleaseSearchViewModelIs(
        Func<ReleaseSearchableDocumentBuilder, ReleaseSearchableDocumentBuilder> modifyReleaseSearchViewModel
    )
    {
        modifyReleaseSearchViewModel(_releaseSearchableDocumentBuilder);
        return this;
    }

    public ContentApiClientMockBuilder WhereThemeHasPublications(params PublicationInfo[] publications)
    {
        _publicationsForTheme = publications;
        return this;
    }

    public ContentApiClientMockBuilder WherePublicationHasReleases(params ReleaseInfo[] releases)
    {
        _releasesForPublication = releases;
        return this;
    }

    public ContentApiClientMockBuilder WhereHasPublications(params PublicationInfo[] publications)
    {
        _publications = publications;
        return this;
    }

    public ContentApiClientMockBuilder WhereGetReleaseSummaryReturns(
        string publicationSlug,
        string releaseSlug,
        ReleaseSummary releaseSummary
    )
    {
        _mock
            .Setup(m => m.GetReleaseSummary(publicationSlug, releaseSlug, It.IsAny<CancellationToken>()))
            .ReturnsAsync(releaseSummary);
        return this;
    }

    public ContentApiClientMockBuilder WhereGetReleaseSummaryThrows(
        string publicationSlug,
        string releaseSlug,
        Exception? exception = null
    )
    {
        _mock
            .Setup(m => m.GetReleaseSummary(publicationSlug, releaseSlug, It.IsAny<CancellationToken>()))
            .Throws(
                (string ps, string rs, CancellationToken _) =>
                    new UnableToGetReleaseSummaryForPublicationException(
                        ps,
                        rs,
                        "This is a test exception. GetReleaseSummary error."
                    )
            );

        return this;
    }

    public Asserter Assert { get; }

    public class Asserter(Mock<IContentApiClient> mock)
    {
        public void ContentWasLoadedFor(string publicationSlug)
        {
            mock.Verify(
                m => m.GetPublicationLatestReleaseSearchableDocument(publicationSlug, It.IsAny<CancellationToken>()),
                Times.Once
            );
        }

        public void PublicationsRequestedForThemeId(Guid expectedThemeId)
        {
            mock.Verify(m => m.GetPublicationsForTheme(expectedThemeId, It.IsAny<CancellationToken>()), Times.Once);
        }

        public void ReleasesRequestedForPublication(string publicationSlug)
        {
            mock.Verify(m => m.GetReleasesForPublication(publicationSlug, It.IsAny<CancellationToken>()), Times.Once);
        }

        public void ReleaseSummaryRequestedForPublication(string publicationSlug, string releaseSlug)
        {
            mock.Verify(
                m => m.GetReleaseSummary(publicationSlug, releaseSlug, It.IsAny<CancellationToken>()),
                Times.Once
            );
        }
    }
}
