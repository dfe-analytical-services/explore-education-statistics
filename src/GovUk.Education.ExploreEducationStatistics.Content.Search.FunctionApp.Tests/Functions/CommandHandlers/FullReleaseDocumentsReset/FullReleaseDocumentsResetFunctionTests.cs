using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Domain;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.FullReleaseDocumentsReset;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.RefreshReleaseDocument.Dto;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Builders;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Functions.CommandHandlers.FullReleaseDocumentsReset;

public class FullReleaseDocumentsResetFunctionTests
{
    private readonly FullReleaseDocumentResetterMockBuilder _fullReleaseDocumentResetter = new();

    // We know this is ignored so little point in creating a mock request
    private readonly HttpRequest _httpRequest = null!;

    private FullReleaseDocumentsResetFunction GetSut() =>
        new(_fullReleaseDocumentResetter.Build(), new TestableCommandHandler());

    [Fact]
    public void CanInstantiateSut() => Assert.NotNull(GetSut());

    [Fact]
    public async Task WhenFullReleaseDocumentsReset_ThenFullReleaseDocumentResetterPerformsReset()
    {
        // Arrange
        var sut = GetSut();

        // Act
        await sut.FullReleaseDocumentsReset(_httpRequest, new FunctionContextMockBuilder().Build());

        // Assert
        _fullReleaseDocumentResetter.Assert.PerformResetWasCalled();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    public async Task WhenFullReleaseDocumentsReset_ThenRefreshReleaseDocumentCommandsReturned(int numberOfPublications)
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
        _fullReleaseDocumentResetter.WherePublicationsReturnedAre(publications);

        var sut = GetSut();

        // Act
        var output = await sut.FullReleaseDocumentsReset(_httpRequest, new FunctionContextMockBuilder().Build());

        // Assert
        var expected = publications.Select(publication => new RefreshReleaseDocumentMessageDto
        {
            PublicationSlug = publication.PublicationSlug,
        });
        Assert.Equal(expected, output.RefreshReleaseDocumentMessages);
    }

    [Fact]
    public async Task WhenFullReleaseDocumentsReset_ThenCountsReturnedInHttpResult()
    {
        // Arrange
        _fullReleaseDocumentResetter
            .WherePublicationsReturnedAre([
                new PublicationInfo { PublicationSlug = "publication-1", LatestReleaseSlug = "release-1" },
                new PublicationInfo { PublicationSlug = "publication-2", LatestReleaseSlug = "release-2" },
            ])
            .WhereRemovedBlobCountIs(5);

        var sut = GetSut();

        // Act
        var output = await sut.FullReleaseDocumentsReset(_httpRequest, new FunctionContextMockBuilder().Build());

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(output.HttpResult);
        Assert.Equal(new FullReleaseDocumentsResetResult(RemovedBlobCount: 5, PublicationCount: 2), okResult.Value);
    }
}
