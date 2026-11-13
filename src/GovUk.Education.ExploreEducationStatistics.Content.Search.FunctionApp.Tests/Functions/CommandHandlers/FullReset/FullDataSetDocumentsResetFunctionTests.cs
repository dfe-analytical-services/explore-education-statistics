using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Domain;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.FullReset;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.RefreshPublicationDataSetDocuments.Dto;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Builders;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Functions.CommandHandlers.FullReset;

public class FullDataSetDocumentsResetFunctionTests
{
    private readonly FullDataSetDocumentResetterMockBuilder _fullDataSetDocumentResetter = new();

    // We know this is ignored so little point in creating a mock request
    private readonly HttpRequest _httpRequest = null!;

    private FullDataSetDocumentsResetFunction GetSut() =>
        new(_fullDataSetDocumentResetter.Build(), new TestableCommandHandler());

    [Fact]
    public void CanInstantiateSut() => Assert.NotNull(GetSut());

    [Fact]
    public async Task WhenFullDataSetDocumentsReset_ThenFullDataSetDocumentResetterPerformsReset()
    {
        // Arrange
        var sut = GetSut();

        // Act
        await sut.FullDataSetDocumentsReset(_httpRequest, new FunctionContextMockBuilder().Build());

        // Assert
        _fullDataSetDocumentResetter.Assert.PerformResetWasCalled();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(10)]
    public async Task WhenFullDataSetDocumentsReset_ThenRefreshPublicationDataSetDocumentsCommandsReturned(
        int numberOfPublications
    )
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
        _fullDataSetDocumentResetter.WherePublicationsReturnedAre(publications);

        var sut = GetSut();

        // Act
        var output = await sut.FullDataSetDocumentsReset(_httpRequest, new FunctionContextMockBuilder().Build());

        // Assert
        var expected = publications.Select(publication => new RefreshPublicationDataSetDocumentsMessageDto
        {
            PublicationId = publication.PublicationId,
            PublicationSlug = publication.PublicationSlug,
        });
        Assert.Equal(expected, output.RefreshPublicationDataSetDocumentsMessages);
    }

    [Fact]
    public async Task WhenFullDataSetDocumentsReset_ThenCountsReturnedInHttpResult()
    {
        // Arrange
        _fullDataSetDocumentResetter
            .WherePublicationsReturnedAre(
                new DataSetDocumentPublication { PublicationId = Guid.NewGuid(), PublicationSlug = "publication-1" },
                new DataSetDocumentPublication { PublicationId = Guid.NewGuid(), PublicationSlug = "publication-2" }
            )
            .WhereRemovedBlobCountIs(5);

        var sut = GetSut();

        // Act
        var output = await sut.FullDataSetDocumentsReset(_httpRequest, new FunctionContextMockBuilder().Build());

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(output.HttpResult);
        Assert.Equal(new FullDataSetDocumentsResetResult(RemovedBlobCount: 5, PublicationCount: 2), okResult.Value);
    }
}
