using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.RefreshSearchableDocument.Dto;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.ReindexSearchableDocuments;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Builders;
using Microsoft.Extensions.Logging.Abstractions;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Functions.ReindexSearchableDocuments;

public class ReindexSearchableDocumentsFunctionTests
{
    private readonly SearchIndexClientMockBuilder _searchIndexClientMockBuilder = new();
    
    private ReindexSearchableDocumentsFunction GetSut() => new(new NullLogger<ReindexSearchableDocumentsFunction>(),
                                                               _searchIndexClientMockBuilder.Build());
    
    [Fact]
    public void Can_instantiate_SUT() => Assert.NotNull(GetSut());

    [Fact]
    public async Task WhenMessageReceived_ThenIndexerRun()
    {
        // ARRANGE
        var sut = GetSut();
        var message = new SearchableDocumentCreatedMessageDto();

        // ACT
        await sut.ReindexSearchableDocuments(message, new FunctionContextMockBuilder().Build());
        
        // ASSERT
        _searchIndexClientMockBuilder.Assert.IndexerRun();
    }
}
