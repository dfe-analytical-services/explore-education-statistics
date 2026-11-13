using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Domain;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Exceptions;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.RemoveDataSetDocuments;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.ResetSearchableDocuments;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Builders;
using Microsoft.Extensions.Logging.Abstractions;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Services.ResetSearchableDocuments;

public class FullDataSetDocumentResetterTests
{
    private readonly ContentApiClientMockBuilder _contentApi = new();
    private readonly DataSetDocumentRemoverMockBuilder _dataSetDocumentRemover = new();

    private IFullDataSetDocumentResetter GetSut() =>
        new FullDataSetDocumentResetter(
            _contentApi.Build(),
            _dataSetDocumentRemover.Build(),
            new NullLogger<FullDataSetDocumentResetter>()
        );

    [Fact]
    public void CanInstantiateSut() => Assert.NotNull(GetSut());

    [Fact]
    public async Task WhenPerformReset_ThenAllDataSetDocumentsAreRemoved()
    {
        // Arrange
        _dataSetDocumentRemover.WhereRemoveAllResponseIs(new RemoveDataSetDocumentsResponse(["blob-1", "blob-2"]));

        var sut = GetSut();

        // Act
        var response = await sut.PerformReset();

        // Assert
        _dataSetDocumentRemover.Assert.AllDataSetDocumentsRemoved();
        Assert.Equal(2, response.RemovedBlobCount);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(10)]
    public async Task WhenPerformReset_ThenReturnsPublicationsWhichHaveDataSetDocuments(int numberOfPublications)
    {
        // Arrange
        var publications = Enumerable
            .Range(0, numberOfPublications)
            .Select(i => new DataSetDocumentPublication
            {
                PublicationId = Guid.NewGuid(),
                PublicationSlug = $"publication-slug-{i}",
            })
            .ToArray();

        _contentApi.WhereHasDataSetDocumentPublications(publications);

        var sut = GetSut();

        // Act
        var response = await sut.PerformReset();

        // Assert
        Assert.Equal(publications, response.Publications);
    }

    [Fact]
    public async Task WhenPublicationsCannotBeRetrieved_ThenNoDataSetDocumentsAreRemoved()
    {
        // Arrange
        _contentApi.WhereGetDataSetDocumentPublicationsThrows();

        var sut = GetSut();

        // Act
        await Assert.ThrowsAsync<UnableToGetDataSetDocumentPublicationsException>(() => sut.PerformReset());

        // Assert
        _dataSetDocumentRemover.Assert.AllDataSetDocumentsNotRemoved();
    }
}
