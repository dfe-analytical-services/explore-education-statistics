using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Domain;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.ResetSearchableDocuments;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Builders;
using Microsoft.Extensions.Logging.Abstractions;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Services.ResetSearchableDocuments;

public class FullSearchableDocumentResetterTests
{
    private readonly ContentApiClientMockBuilder _contentApi = new();
    private readonly SearchableDocumentRemoverMockBuilder _searchableDocumentRemover = new();

    private IFullSearchableDocumentResetter GetSut() =>
        new FullSearchableDocumentResetter(
            _contentApi.Build(),
            _searchableDocumentRemover.Build(),
            new NullLogger<FullSearchableDocumentResetter>()
        );

    [Fact]
    public void Can_instantiate_SUT() => Assert.NotNull(GetSut());

    [Fact]
    public async Task WhenPerformReset_ThenAllSearchableDocumentsAreRemoved()
    {
        // Arrange
        var sut = GetSut();

        // Act
        await sut.PerformReset();

        // Assert
        _searchableDocumentRemover.Assert.AllSearchableDocumentsRemoved();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    public async Task WhenPerformFullReset_ThenReturnsListOfAllPublications(
        int numberOfPublications
    )
    {
        // Arrange
        var publications = Enumerable
            .Range(0, numberOfPublications)
            .Select(i => new PublicationInfo
            {
                PublicationSlug = $"publication-slug-{i}",
                LatestReleaseSlug = $"release-slug-{i}",
            })
            .ToArray();

        _contentApi.WhereHasPublications(publications);

        var sut = GetSut();

        // Act
        var response = await sut.PerformReset();

        // Assert
        Assert.NotNull(response);
        var actual = response.AllPublications;
        Assert.Equal(publications, actual);
    }
}
