using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Domain;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.FullReset;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Builders;
using Microsoft.AspNetCore.Http;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Functions.FullReset;

public class FullResetFunctionTests
{
    private readonly FullSearchableDocumentResetterMockBuilder _fullSearchableDocumentResetter = new();
    
    private FullResetFunction GetSut() => new(
        _fullSearchableDocumentResetter.Build(), 
        new TestableCommandHandler());
    
    [Fact]
    public void Can_instantiate_Sut() => Assert.NotNull(GetSut());

    [Fact]
    public async Task WhenFullSearchableDocumentsReset_ThenFullSearchableDocumentResetterPerformsReset()
    {
        // Arrange
        var sut = GetSut();

        // We know this is ignored so little point in creating a mock request
        HttpRequest httpRequest = null!;
        var functionContext = new FunctionContextMockBuilder().Build();

        // Act
        await sut.FullSearchableDocumentsReset(httpRequest, functionContext);

        // Assert
        _fullSearchableDocumentResetter.Assert.PerformResetWasCalled();
    }
    
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    public async Task WhenFullSearchableDocumentsReset_ThenResetPublicationSearchableDocumentCommandsReturned(int numberOfPublications)
    {
        // Arrange
        var publications = Enumerable.Range(0, numberOfPublications)
            .Select(i => new PublicationInfo{ PublicationSlug = $"publication-slug-{i}"})
            .ToArray();
        _fullSearchableDocumentResetter.WherePublicationsReturnedAre(publications);
            
        var sut = GetSut();

        // We know this is ignored so little point in creating a mock request
        HttpRequest httpRequest = null!;
        var functionContext = new FunctionContextMockBuilder().Build();

        // Act
        var response = await sut.FullSearchableDocumentsReset(httpRequest, functionContext);

        // Assert
        Assert.NotNull(response);
        var actual = response.Select(dto => dto.PublicationSlug);
        var expected = publications.Select(p => p.PublicationSlug);
        Assert.Equal(expected, actual);
    }
}
