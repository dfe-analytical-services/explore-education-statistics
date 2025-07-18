﻿using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.RemoveSearchableDocument;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.RemoveSearchableDocument.Dto;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Builders;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.TheoryDataHelpers;
using Microsoft.Extensions.Logging.Abstractions;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Functions.CommandHandlers.RemoveSearchableDocument;

public class RemoveSearchableDocumentTests
{
    private readonly SearchableDocumentRemoverMockBuilder _searchableDocumentRemoverMockBuilder = new();
    private RemoveSearchableDocumentFunction GetSut() => new(
        new NullLogger<RemoveSearchableDocumentFunction>(),
        _searchableDocumentRemoverMockBuilder.Build(), 
        new TestableCommandHandler());

    [Fact]
    public void CanInstantiateSut() => Assert.NotNull(GetSut());

    [Fact]
    public async Task WhenMessageContainsReleaseId_ThenDocumentRemoverCalled()
    {
        var command = new RemoveSearchableDocumentDto { ReleaseId = Guid.NewGuid() };

        await GetSut().RemoveSearchableDocument(command, new FunctionContextMockBuilder().Build());

        _searchableDocumentRemoverMockBuilder.Assert
            .RemoveSearchableDocumentCalledFor(command.ReleaseId.Value);
    }


    [Theory]
    [MemberData(nameof(TheoryDatas.Blank.Guids), MemberType = typeof(TheoryDatas.Blank))]
    public async Task WhenMessageDoesNotContainReleaseId_ThenDocumentRemoverNotCalled(Guid? missingReleaseId)
    {
        var command = new RemoveSearchableDocumentDto { ReleaseId = missingReleaseId };

        await GetSut().RemoveSearchableDocument(command, new FunctionContextMockBuilder().Build());

        _searchableDocumentRemoverMockBuilder.Assert.RemoveSearchableDocumentNotCalled();
    }
}
