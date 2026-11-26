using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Domain;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Exceptions;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.RemoveReleaseDocuments;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.ResetReleaseDocuments;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Builders;
using Microsoft.Extensions.Logging.Abstractions;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Services.ResetReleaseDocuments;

public class FullReleaseDocumentResetterTests
{
    private readonly ContentApiClientMockBuilder _contentApi = new();
    private readonly ReleaseDocumentRemoverMockBuilder _releaseDocumentRemover = new();

    private IFullReleaseDocumentResetter GetSut() =>
        new FullReleaseDocumentResetter(
            _contentApi.Build(),
            _releaseDocumentRemover.Build(),
            new NullLogger<FullReleaseDocumentResetter>()
        );

    [Fact]
    public void Can_instantiate_SUT() => Assert.NotNull(GetSut());

    [Fact]
    public async Task WhenPerformReset_ThenAllReleaseDocumentsAreRemoved()
    {
        // Arrange
        _releaseDocumentRemover.WhereRemoveAllResponseIs(new RemoveAllReleaseDocumentsResponse(["blob-1", "blob-2"]));

        var sut = GetSut();

        // Act
        var response = await sut.PerformReset();

        // Assert
        _releaseDocumentRemover.Assert.AllReleaseDocumentsRemoved();
        Assert.Equal(2, response.RemovedBlobCount);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    public async Task WhenPerformFullReset_ThenReturnsListOfAllPublications(int numberOfPublications)
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
        var actual = response.Publications;
        Assert.Equal(publications, actual);
    }

    [Fact]
    public async Task WhenPublicationsCannotBeRetrieved_ThenNoReleaseDocumentsAreRemoved()
    {
        // Arrange
        _contentApi.WhereGetAllLivePublicationInfosThrows();

        var sut = GetSut();

        // Act
        await Assert.ThrowsAsync<UnableToGetPublicationInfosException>(() => sut.PerformReset());

        // Assert
        _releaseDocumentRemover.Assert.AllReleaseDocumentsNotRemoved();
    }
}
