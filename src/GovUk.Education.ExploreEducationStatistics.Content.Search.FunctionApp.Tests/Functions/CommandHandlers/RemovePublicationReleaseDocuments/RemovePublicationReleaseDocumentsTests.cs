using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.RemovePublicationReleaseDocuments;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.RemovePublicationReleaseDocuments.Dto;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Builders;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.TheoryDataHelpers;
using Microsoft.Extensions.Logging.Abstractions;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Functions.CommandHandlers.RemovePublicationReleaseDocuments;

public class RemovePublicationReleaseDocumentsTests
{
    private readonly SearchableDocumentRemoverMockBuilder _searchableDocumentRemoverMockBuilder = new();

    private RemovePublicationReleaseDocumentsFunction GetSut() =>
        new(
            new NullLogger<RemovePublicationReleaseDocumentsFunction>(),
            _searchableDocumentRemoverMockBuilder.Build(),
            new TestableCommandHandler()
        );

    [Fact]
    public void CanInstantiateSut() => Assert.NotNull(GetSut());

    [Fact]
    public async Task WhenMessageContainsSlug_ThenDocumentRemoverCalled()
    {
        var command = new RemovePublicationReleaseDocumentsDto { PublicationSlug = "publication-slug" };

        await GetSut().RemovePublicationReleaseDocuments(command, new FunctionContextMockBuilder().Build());

        _searchableDocumentRemoverMockBuilder.Assert.RemovePublicationSearchableDocumentsCalledFor(
            command.PublicationSlug
        );
    }

    [Theory]
    [MemberData(nameof(TheoryDatas.Blank.Strings), MemberType = typeof(TheoryDatas.Blank))]
    public async Task WhenMessageDoesNotContainSlug_ThenDocumentRemoverNotCalled(string? publicationSlug)
    {
        var command = new RemovePublicationReleaseDocumentsDto { PublicationSlug = publicationSlug };

        await GetSut().RemovePublicationReleaseDocuments(command, new FunctionContextMockBuilder().Build());

        _searchableDocumentRemoverMockBuilder.Assert.RemovePublicationSearchableDocumentsNotCalled();
    }
}
