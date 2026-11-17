using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.RefreshReleaseDocument.Dto;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.ReindexReleaseDocuments;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Options;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Builders;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Functions.CommandHandlers.ReindexReleaseDocuments;

public class ReindexReleaseDocumentsFunctionTests
{
    private const string ReleaseIndexerName = "release-indexer-name";
    private readonly SearchIndexClientMockBuilder _searchIndexClientMockBuilder = new();

    private ReindexReleaseDocumentsFunction GetSut() =>
        new(
            _searchIndexClientMockBuilder.Build(),
            new TestableCommandHandler(),
            Microsoft.Extensions.Options.Options.Create(
                new AzureSearchOptions { ReleaseIndexerName = ReleaseIndexerName }
            )
        );

    [Fact]
    public void Can_instantiate_SUT() => Assert.NotNull(GetSut());

    [Fact]
    public async Task WhenMessageReceived_ThenReleaseIndexerRun()
    {
        // ARRANGE
        var sut = GetSut();
        var message = new ReleaseDocumentCreatedMessageDto();

        // ACT
        await sut.ReindexReleaseDocuments(message, new FunctionContextMockBuilder().Build());

        // ASSERT
        _searchIndexClientMockBuilder.Assert.IndexerRun(ReleaseIndexerName);
    }
}
