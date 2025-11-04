using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Domain;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.CheckSearchableDocuments;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Builders;
using Microsoft.Extensions.Logging.Abstractions;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Services.CheckSearchableDocuments;

public class ReleaseSummaryRetrieverTests
{
    private readonly ContentApiClientMockBuilder _contentApiClient = new();

    private ReleaseSummaryRetriever GetSut() =>
        new(() => _contentApiClient.Build(), new NullLogger<ReleaseSummaryRetriever>());

    [Fact]
    public void CanInstantiateSut() => Assert.NotNull(GetSut());

    [Fact]
    public async Task GivenSomePublications_WhenGetAllPublishedReleaseSummaries_ThenReturnsPublicationsAndReleaseInformation()
    {
        // Arrange
        var publications = Enumerable
            .Range(0, 3)
            .Select(i => new PublicationInfo
            {
                PublicationSlug = $"publication-slug-{i}",
                LatestReleaseSlug = $"release-slug-{i}",
            })
            .ToArray();

        _contentApiClient.WhereHasPublications(publications);

        var sut = GetSut();

        // Act
        var actual = await sut.GetAllPublishedReleaseSummaries();

        // Assert
        Assert.Equal(publications.Select(p => p.PublicationSlug), actual.Select(r => r.PublicationSlug));
        Assert.All(
            publications,
            publication =>
                _contentApiClient.Assert.ReleaseSummaryRequestedForPublication(
                    publication.PublicationSlug,
                    publication.LatestReleaseSlug
                )
        );
    }

    [Fact]
    public async Task GivenSomePublicationsIncludingABrokenOne_WhenGetAllPublishedReleaseSummaries_ThenReturnsPublicationsAndReleaseInformation()
    {
        // Arrange
        var publications = Enumerable
            .Range(0, 3)
            .Select(i => new PublicationInfo
            {
                PublicationSlug = $"publication-slug-{i}",
                LatestReleaseSlug = $"release-slug-{i}",
            })
            .ToArray();

        _contentApiClient.WhereHasPublications(publications);

        // Given duff data, this call can fail
        _contentApiClient.WhereGetReleaseSummaryThrows(
            publications[1].PublicationSlug,
            publications[1].LatestReleaseSlug
        );

        var sut = GetSut();

        // Act
        var actual = await sut.GetAllPublishedReleaseSummaries();

        // Assert
        var validPublications = publications.Except([publications[1]]).ToArray();
        Assert.Equal(validPublications.Select(p => p.PublicationSlug), actual.Select(r => r.PublicationSlug));

        Assert.All(
            publications,
            publication =>
                _contentApiClient.Assert.ReleaseSummaryRequestedForPublication(
                    publication.PublicationSlug,
                    publication.LatestReleaseSlug
                )
        );
    }
}
