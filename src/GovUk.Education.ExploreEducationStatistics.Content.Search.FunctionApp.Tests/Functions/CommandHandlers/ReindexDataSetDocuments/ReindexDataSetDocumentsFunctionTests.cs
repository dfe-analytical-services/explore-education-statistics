using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.ReindexDataSetDocuments;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.ReindexDataSetDocuments.Dto;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Options;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Builders;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Functions.CommandHandlers.ReindexDataSetDocuments;

public class ReindexDataSetDocumentsFunctionTests
{
    private const string DataSetIndexerName = "data-set-indexer-name";

    private readonly SearchIndexClientMockBuilder _searchIndexClientMockBuilder = new();

    private ReindexDataSetDocumentsFunction GetSut() =>
        new(
            _searchIndexClientMockBuilder.Build(),
            new TestableCommandHandler(),
            Microsoft.Extensions.Options.Options.Create(
                new AzureSearchOptions
                {
                    ReleaseIndexerName = "release-indexer-name",
                    DataSetIndexerName = DataSetIndexerName,
                }
            )
        );

    [Fact]
    public void CanInstantiateSut() => Assert.NotNull(GetSut());

    [Fact]
    public async Task WhenMessageReceived_ThenDataSetIndexerRun()
    {
        await GetSut()
            .ReindexDataSetDocuments(new DataSetDocumentsChangedMessageDto(), new FunctionContextMockBuilder().Build());

        _searchIndexClientMockBuilder.Assert.IndexerRun(DataSetIndexerName);
    }
}
