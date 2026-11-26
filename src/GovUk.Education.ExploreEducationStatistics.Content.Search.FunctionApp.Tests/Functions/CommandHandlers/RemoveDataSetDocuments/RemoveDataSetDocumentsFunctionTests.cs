using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.ReindexDataSetDocuments.Dto;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.RemoveDataSetDocuments;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.RemoveDataSetDocuments.Dto;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.RemoveDataSetDocuments;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Builders;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.TheoryDataHelpers;
using Microsoft.Extensions.Logging.Abstractions;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Functions.CommandHandlers.RemoveDataSetDocuments;

public class RemoveDataSetDocumentsFunctionTests
{
    private static readonly Guid PublicationId = Guid.NewGuid();
    private static readonly Guid ReleaseId = Guid.NewGuid();
    private static readonly Guid FileId = Guid.NewGuid();

    private readonly DataSetDocumentRemoverMockBuilder _dataSetDocumentRemoverMockBuilder = new();

    public RemoveDataSetDocumentsFunctionTests()
    {
        _dataSetDocumentRemoverMockBuilder.WhereResponseIs(new RemoveDataSetDocumentsResponse(["removed-blob"]));
    }

    private RemoveDataSetDocumentsFunction GetSut() =>
        new(
            new NullLogger<RemoveDataSetDocumentsFunction>(),
            _dataSetDocumentRemoverMockBuilder.Build(),
            new TestableCommandHandler()
        );

    [Fact]
    public void CanInstantiateSut() => Assert.NotNull(GetSut());

    [Fact]
    public async Task WhenMessageHasPublicationIdOnly_ThenDataSetDocumentsOfPublicationRemoved()
    {
        var message = new RemoveDataSetDocumentsMessageDto { PublicationId = PublicationId };

        await GetSut().RemoveDataSetDocuments(message, new FunctionContextMockBuilder().Build());

        _dataSetDocumentRemoverMockBuilder.Assert.RemoveDataSetDocumentsCalledFor(request =>
            request.PublicationId == PublicationId
            && request.ReleaseId == null
            && request.FileId == null
            && request.Except.Count == 0
        );
    }

    [Fact]
    public async Task WhenMessageHasReleaseId_ThenDataSetDocumentsOfReleaseRemoved()
    {
        var message = new RemoveDataSetDocumentsMessageDto { PublicationId = PublicationId, ReleaseId = ReleaseId };

        await GetSut().RemoveDataSetDocuments(message, new FunctionContextMockBuilder().Build());

        _dataSetDocumentRemoverMockBuilder.Assert.RemoveDataSetDocumentsCalledFor(request =>
            request.PublicationId == PublicationId
            && request.ReleaseId == ReleaseId
            && request.FileId == null
            && request.Except.Count == 0
        );
    }

    [Fact]
    public async Task WhenMessageHasFileId_ThenDataSetDocumentOfDataFileRemoved()
    {
        var message = new RemoveDataSetDocumentsMessageDto
        {
            PublicationId = PublicationId,
            ReleaseId = ReleaseId,
            FileId = FileId,
        };

        await GetSut().RemoveDataSetDocuments(message, new FunctionContextMockBuilder().Build());

        _dataSetDocumentRemoverMockBuilder.Assert.RemoveDataSetDocumentsCalledFor(request =>
            request.PublicationId == PublicationId
            && request.ReleaseId == ReleaseId
            && request.FileId == FileId
            && request.Except.Count == 0
        );
    }

    [Fact]
    public async Task WhenDataSetDocumentsRemoved_ThenDataSetDocumentsChangedMessageReturned()
    {
        var message = new RemoveDataSetDocumentsMessageDto { PublicationId = PublicationId, ReleaseId = ReleaseId };

        var response = await GetSut().RemoveDataSetDocuments(message, new FunctionContextMockBuilder().Build());

        Assert.Equal(
            [new DataSetDocumentsChangedMessageDto { PublicationId = PublicationId, ReleaseId = ReleaseId }],
            response
        );
    }

    [Fact]
    public async Task WhenNoDataSetDocumentsRemoved_ThenNoMessageReturned()
    {
        _dataSetDocumentRemoverMockBuilder.WhereResponseIs(new RemoveDataSetDocumentsResponse([]));
        var message = new RemoveDataSetDocumentsMessageDto { PublicationId = PublicationId };

        var response = await GetSut().RemoveDataSetDocuments(message, new FunctionContextMockBuilder().Build());

        Assert.Empty(response);
    }

    [Theory]
    [MemberData(nameof(TheoryDatas.Blank.Guids), MemberType = typeof(TheoryDatas.Blank))]
    public async Task WhenMessageHasNoPublicationId_ThenDataSetDocumentsNotRemoved(Guid? blankPublicationId)
    {
        var message = new RemoveDataSetDocumentsMessageDto
        {
            PublicationId = blankPublicationId,
            ReleaseId = ReleaseId,
            FileId = FileId,
        };

        var response = await GetSut().RemoveDataSetDocuments(message, new FunctionContextMockBuilder().Build());

        Assert.Empty(response);
        _dataSetDocumentRemoverMockBuilder.Assert.RemoveDataSetDocumentsNotCalled();
    }

    [Theory]
    [MemberData(nameof(TheoryDatas.Blank.Guids), MemberType = typeof(TheoryDatas.Blank))]
    public async Task WhenMessageHasFileIdButNoReleaseId_ThenDataSetDocumentsNotRemoved(Guid? blankReleaseId)
    {
        var message = new RemoveDataSetDocumentsMessageDto
        {
            PublicationId = PublicationId,
            ReleaseId = blankReleaseId,
            FileId = FileId,
        };

        var response = await GetSut().RemoveDataSetDocuments(message, new FunctionContextMockBuilder().Build());

        Assert.Empty(response);
        _dataSetDocumentRemoverMockBuilder.Assert.RemoveDataSetDocumentsNotCalled();
    }
}
