using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.
    RemovePublicationSearchableDocuments;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.
    RemovePublicationSearchableDocuments.Dto;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Builders;
using Microsoft.Extensions.Logging.Abstractions;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Functions.
    RemovePublicationSearchableDocuments;

public class RemovePublicationSearchableDocumentsTests
{
    private readonly SearchableDocumentRemoverMockBuilder _searchableDocumentRemoverMockBuilder = new();

    private RemovePublicationSearchableDocumentsFunction GetSut() => new(
        new NullLogger<RemovePublicationSearchableDocumentsFunction>(),
        _searchableDocumentRemoverMockBuilder.Build());

    [Fact]
    public void CanInstantiateSut() => Assert.NotNull(GetSut());

    [Fact]
    public async Task WhenMessageContainsSlug_ThenDocumentRemoverCalled()
    {
        var command = new RemovePublicationSearchableDocumentsDto { PublicationSlug = "publication-slug" };

        await GetSut().RemovePublicationSearchableDocuments(command, new FunctionContextMockBuilder().Build());

        _searchableDocumentRemoverMockBuilder.Assert.RemovePublicationSearchableDocumentsCalledFor(
            command.PublicationSlug);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task WhenMessageDoesNotContainSlug_ThenDocumentRemoverNotCalled(string? publicationSlug)
    {
        var command = new RemovePublicationSearchableDocumentsDto { PublicationSlug = publicationSlug };

        await GetSut().RemovePublicationSearchableDocuments(command, new FunctionContextMockBuilder().Build());

        _searchableDocumentRemoverMockBuilder.Assert.RemovePublicationSearchableDocumentsNotCalled();
    }
}
