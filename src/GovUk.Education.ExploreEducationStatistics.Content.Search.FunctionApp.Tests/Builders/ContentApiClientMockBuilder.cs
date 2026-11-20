using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.ContentApi;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Domain;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Exceptions;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Builders;

internal class ContentApiClientMockBuilder
{
    private readonly Mock<IContentApiClient> _mock = new(MockBehavior.Strict);
    private readonly ReleaseDocumentBuilder _releaseDocumentBuilder = new();
    private ReleaseDocument? _releaseDocument;
    private PublicationInfo[]? _publicationsForTheme;
    private Guid[]? _publicationReleaseIds;
    private PublicationInfo[]? _publications;
    private DataSetDocumentKey[]? _dataSetDocumentKeys;
    private DataSetDocument[]? _releaseDataSetDocuments;
    private DataSetDocument[]? _publicationDataSetDocuments;

    public ContentApiClientMockBuilder()
    {
        Assert = new Asserter(_mock);

        _mock
            .Setup(m => m.GetDataSetDocumentKeys(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => _dataSetDocumentKeys ?? []);

        _mock
            .Setup(m =>
                m.GetReleaseDataSetDocuments(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(() => _releaseDataSetDocuments ?? []);

        _mock
            .Setup(m => m.GetPublicationDataSetDocuments(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => _publicationDataSetDocuments ?? []);

        _mock
            .Setup(m => m.GetPublicationLatestReleaseDocument(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => _releaseDocument ?? _releaseDocumentBuilder.Build());

        _mock
            .Setup(m => m.GetPublicationsForTheme(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => _publicationsForTheme ?? []);

        _mock
            .Setup(m => m.GetPublicationReleaseIds(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => _publicationReleaseIds ?? []);

        _mock
            .Setup(m => m.GetAllLivePublicationInfos(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => _publications ?? []);

        _mock
            .Setup(m =>
                m.GetReleaseVersionSummary(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(
                (string publicationSlug, string releaseSlug, CancellationToken _) =>
                    new ReleaseVersionSummary
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

    public ContentApiClientMockBuilder WhereReleaseDocumentIs(ReleaseDocument releaseDocument)
    {
        _releaseDocument = releaseDocument;
        return this;
    }

    public ContentApiClientMockBuilder WhereReleaseDocumentIs(
        Func<ReleaseDocumentBuilder, ReleaseDocumentBuilder> modifyReleaseDocument
    )
    {
        modifyReleaseDocument(_releaseDocumentBuilder);
        return this;
    }

    public ContentApiClientMockBuilder WhereThemeHasPublications(params PublicationInfo[] publications)
    {
        _publicationsForTheme = publications;
        return this;
    }

    public ContentApiClientMockBuilder WherePublicationHasReleaseIds(params Guid[] releaseIds)
    {
        _publicationReleaseIds = releaseIds;
        return this;
    }

    public ContentApiClientMockBuilder WhereHasPublications(params PublicationInfo[] publications)
    {
        _publications = publications;
        return this;
    }

    public ContentApiClientMockBuilder WhereHasDataSetDocumentKeys(params DataSetDocumentKey[] dataSetDocumentKeys)
    {
        _dataSetDocumentKeys = dataSetDocumentKeys;
        return this;
    }

    public ContentApiClientMockBuilder WhereReleaseHasDataSetDocuments(params DataSetDocument[] dataSetDocuments)
    {
        _releaseDataSetDocuments = dataSetDocuments;
        return this;
    }

    public ContentApiClientMockBuilder WherePublicationHasDataSetDocuments(params DataSetDocument[] dataSetDocuments)
    {
        _publicationDataSetDocuments = dataSetDocuments;
        return this;
    }

    public ContentApiClientMockBuilder WhereGetReleaseVersionSummaryReturns(
        string publicationSlug,
        string releaseSlug,
        ReleaseVersionSummary releaseVersionSummary
    )
    {
        _mock
            .Setup(m => m.GetReleaseVersionSummary(publicationSlug, releaseSlug, It.IsAny<CancellationToken>()))
            .ReturnsAsync(releaseVersionSummary);
        return this;
    }

    public ContentApiClientMockBuilder WhereGetReleaseVersionSummaryThrows(
        string publicationSlug,
        string releaseSlug,
        Exception? exception = null
    )
    {
        _mock
            .Setup(m => m.GetReleaseVersionSummary(publicationSlug, releaseSlug, It.IsAny<CancellationToken>()))
            .Throws(
                (string ps, string rs, CancellationToken _) =>
                    new UnableToGetReleaseVersionSummaryException(
                        ps,
                        rs,
                        "This is a test exception. GetReleaseVersionSummary error."
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
                m => m.GetPublicationLatestReleaseDocument(publicationSlug, It.IsAny<CancellationToken>()),
                Times.Once
            );
        }

        public void PublicationsRequestedForThemeId(Guid expectedThemeId)
        {
            mock.Verify(m => m.GetPublicationsForTheme(expectedThemeId, It.IsAny<CancellationToken>()), Times.Once);
        }

        public void ReleaseIdsRequestedForPublication(string publicationSlug)
        {
            mock.Verify(m => m.GetPublicationReleaseIds(publicationSlug, It.IsAny<CancellationToken>()), Times.Once);
        }

        public void DataSetDocumentsRequestedForRelease(string publicationSlug, string releaseSlug)
        {
            mock.Verify(
                m => m.GetReleaseDataSetDocuments(publicationSlug, releaseSlug, It.IsAny<CancellationToken>()),
                Times.Once
            );
        }

        public void DataSetDocumentsRequestedForPublication(string publicationSlug)
        {
            mock.Verify(
                m => m.GetPublicationDataSetDocuments(publicationSlug, It.IsAny<CancellationToken>()),
                Times.Once
            );
        }

        public void ReleaseVersionSummaryRequestedForPublication(string publicationSlug, string releaseSlug)
        {
            mock.Verify(
                m => m.GetReleaseVersionSummary(publicationSlug, releaseSlug, It.IsAny<CancellationToken>()),
                Times.Once
            );
        }
    }
}
