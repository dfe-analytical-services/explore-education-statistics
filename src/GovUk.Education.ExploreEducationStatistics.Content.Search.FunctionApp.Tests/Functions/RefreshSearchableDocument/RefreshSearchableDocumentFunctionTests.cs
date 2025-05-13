using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.RefreshSearchableDocument;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.RefreshSearchableDocument.Dto;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.CreateSearchableDocuments;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Builders;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.TheoryDataHelpers;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Functions.RefreshSearchableDocument;

public class RefreshSearchableDocumentFunctionTests
{
    private readonly SearchableDocumentCreatorMockBuilder _searchableDocumentCreatorMockBuilder = new();
    
    private RefreshSearchableDocumentFunction GetSut() => new(_searchableDocumentCreatorMockBuilder.Build());
    
    [Fact]
    public void Can_instantiate_SUT() => Assert.NotNull(GetSut());

    [Fact]
    public async Task WhenMessageWithPublicationSlug_ThenDocumentCreatorCalled()
    {
        // ARRANGE
        var sut = GetSut();
        var publicationSlug = "publication-slug";
        var command = new RefreshSearchableDocumentMessageDto
        {
            PublicationSlug = publicationSlug
        };
        
        // ACT
        await sut.RefreshSearchableDocument(command, new FunctionContextMockBuilder().Build());

        // ASSERT
        _searchableDocumentCreatorMockBuilder.Assert.CreateSearchableDocumentCalledFor(publicationSlug);
    }
    
    [Theory]
    [MemberData(nameof(TheoryDatas.Blank.Strings), MemberType = typeof(TheoryDatas.Blank))]
    public async Task WhenMessageHasNoPublicationSlug_ThenDocumentCreatorNotCalled(string? blankPublicationSlug)
    {
        // ARRANGE
        var sut = GetSut();
        var command = new RefreshSearchableDocumentMessageDto
        {
            PublicationSlug = blankPublicationSlug
        };
        
        // ACT
        var response = await sut.RefreshSearchableDocument(command, new FunctionContextMockBuilder().Build());

        // ASSERT
        _searchableDocumentCreatorMockBuilder.Assert.CreateSearchableDocumentNotCalled();
    }
    
    
    [Fact]
    public async Task GivenMessageWithPublicationSlug_WhenDocumentCreatorCalled_ThenResponseDataIsReturned()
    {
        // ARRANGE
        var sut = GetSut();
        var publicationSlug = "publication-slug";
        var command = new RefreshSearchableDocumentMessageDto
        {
            PublicationSlug = publicationSlug
        };
        var expectedResponse = new CreatePublicationLatestReleaseSearchableDocumentResponse
        {
            PublicationSlug = publicationSlug,
            ReleaseId = Guid.NewGuid(),
            ReleaseSlug = "release-slug",
            ReleaseVersionId = Guid.NewGuid(),
            BlobName = "blob name"
        };
        _searchableDocumentCreatorMockBuilder.WhereResponseIs(expectedResponse);
        
        // ACT
        var response = await sut.RefreshSearchableDocument(command, new FunctionContextMockBuilder().Build());

        // ASSERT
        Assert.NotNull(response);
        var actual = Assert.Single(response);
        Assert.NotNull(actual);
        Assert.Equal(expectedResponse.PublicationSlug, publicationSlug);
        Assert.Equal(expectedResponse.ReleaseId, actual.ReleaseId);
        Assert.Equal(expectedResponse.ReleaseSlug, actual.ReleaseSlug);
        Assert.Equal(expectedResponse.ReleaseVersionId, actual.ReleaseVersionId);
        Assert.Equal(expectedResponse.BlobName, actual.BlobName);
    }
}
