using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.RemoveReleaseDocument;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.RemoveReleaseDocument.Dto;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Builders;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.TheoryDataHelpers;
using Microsoft.Extensions.Logging.Abstractions;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Functions.CommandHandlers.RemoveReleaseDocument;

public class RemoveReleaseDocumentTests
{
    private readonly SearchableDocumentRemoverMockBuilder _searchableDocumentRemoverMockBuilder = new();

    private RemoveReleaseDocumentFunction GetSut() =>
        new(
            new NullLogger<RemoveReleaseDocumentFunction>(),
            _searchableDocumentRemoverMockBuilder.Build(),
            new TestableCommandHandler()
        );

    [Fact]
    public void CanInstantiateSut() => Assert.NotNull(GetSut());

    [Fact]
    public async Task WhenMessageContainsReleaseId_ThenDocumentRemoverCalled()
    {
        var command = new RemoveReleaseDocumentDto { ReleaseId = Guid.NewGuid() };

        await GetSut().RemoveReleaseDocument(command, new FunctionContextMockBuilder().Build());

        _searchableDocumentRemoverMockBuilder.Assert.RemoveSearchableDocumentCalledFor(command.ReleaseId.Value);
    }

    [Theory]
    [MemberData(nameof(TheoryDatas.Blank.Guids), MemberType = typeof(TheoryDatas.Blank))]
    public async Task WhenMessageDoesNotContainReleaseId_ThenDocumentRemoverNotCalled(Guid? missingReleaseId)
    {
        var command = new RemoveReleaseDocumentDto { ReleaseId = missingReleaseId };

        await GetSut().RemoveReleaseDocument(command, new FunctionContextMockBuilder().Build());

        _searchableDocumentRemoverMockBuilder.Assert.RemoveSearchableDocumentNotCalled();
    }
}
