using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.RefreshReleaseDocument.Dto;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.ReindexReleaseDocuments;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Builders;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Functions.CommandHandlers.ReindexReleaseDocuments;

public class ReindexReleaseDocumentsFunctionTests
{
    private readonly SearchIndexClientMockBuilder _searchIndexClientMockBuilder = new();

    private ReindexReleaseDocumentsFunction GetSut() =>
        new(_searchIndexClientMockBuilder.Build(), new TestableCommandHandler());

    [Fact]
    public void Can_instantiate_SUT() => Assert.NotNull(GetSut());

    [Fact]
    public async Task WhenMessageReceived_ThenIndexerRun()
    {
        // ARRANGE
        var sut = GetSut();
        var message = new ReleaseDocumentCreatedMessageDto();

        // ACT
        await sut.ReindexReleaseDocuments(message, new FunctionContextMockBuilder().Build());

        // ASSERT
        _searchIndexClientMockBuilder.Assert.IndexerRun();
    }
}
