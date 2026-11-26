using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Domain;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.RefreshReleaseDataSetDocuments;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.RefreshReleaseDataSetDocuments.Dto;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.ReindexDataSetDocuments.Dto;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.CreateDataSetDocuments;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.RemoveDataSetDocuments;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Builders;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.TheoryDataHelpers;
using Microsoft.Extensions.Logging.Abstractions;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Functions.CommandHandlers.RefreshReleaseDataSetDocuments;

public class RefreshReleaseDataSetDocumentsFunctionTests
{
    private static readonly RefreshReleaseDataSetDocumentsMessageDto Message = new()
    {
        PublicationId = Guid.NewGuid(),
        PublicationSlug = "publication-slug",
        ReleaseId = Guid.NewGuid(),
        ReleaseSlug = "release-slug",
    };

    private static readonly DataSetDocumentKey CreatedKey = new()
    {
        PublicationId = Message.PublicationId!.Value,
        ReleaseId = Message.ReleaseId!.Value,
        FileId = Guid.NewGuid(),
    };

    private readonly DataSetDocumentCreatorMockBuilder _dataSetDocumentCreatorMockBuilder = new();
    private readonly DataSetDocumentRemoverMockBuilder _dataSetDocumentRemoverMockBuilder = new();

    public RefreshReleaseDataSetDocumentsFunctionTests()
    {
        _dataSetDocumentCreatorMockBuilder.WhereResponseIs(new CreateDataSetDocumentsResponse([CreatedKey]));
    }

    private RefreshReleaseDataSetDocumentsFunction GetSut() =>
        new(
            new NullLogger<RefreshReleaseDataSetDocumentsFunction>(),
            _dataSetDocumentCreatorMockBuilder.Build(),
            _dataSetDocumentRemoverMockBuilder.Build(),
            new TestableCommandHandler()
        );

    [Fact]
    public void CanInstantiateSut() => Assert.NotNull(GetSut());

    [Fact]
    public async Task WhenMessageIdentifiesRelease_ThenDataSetDocumentsOfReleaseCreated()
    {
        await GetSut().RefreshReleaseDataSetDocuments(Message, new FunctionContextMockBuilder().Build());

        _dataSetDocumentCreatorMockBuilder.Assert.CreateReleaseDataSetDocumentsCalledFor(
            new CreateReleaseDataSetDocumentsRequest
            {
                PublicationId = Message.PublicationId!.Value,
                PublicationSlug = "publication-slug",
                ReleaseId = Message.ReleaseId!.Value,
                ReleaseSlug = "release-slug",
            }
        );
    }

    [Fact]
    public async Task WhenMessageIdentifiesRelease_ThenOtherDataSetDocumentsOfReleaseRemoved()
    {
        await GetSut().RefreshReleaseDataSetDocuments(Message, new FunctionContextMockBuilder().Build());

        _dataSetDocumentRemoverMockBuilder.Assert.RemoveDataSetDocumentsCalledFor(request =>
            request.PublicationId == Message.PublicationId
            && request.ReleaseId == Message.ReleaseId
            && request.FileId == null
            && request.Except.SequenceEqual([CreatedKey])
        );
    }

    [Fact]
    public async Task WhenReleaseHasNoDataSetDocuments_ThenAllDataSetDocumentsOfReleaseRemoved()
    {
        _dataSetDocumentCreatorMockBuilder.WhereResponseIs(new CreateDataSetDocumentsResponse([]));

        await GetSut().RefreshReleaseDataSetDocuments(Message, new FunctionContextMockBuilder().Build());

        _dataSetDocumentRemoverMockBuilder.Assert.RemoveDataSetDocumentsCalledFor(request =>
            request.PublicationId == Message.PublicationId
            && request.ReleaseId == Message.ReleaseId
            && request.FileId == null
            && request.Except.Count == 0
        );
    }

    [Fact]
    public async Task WhenCreatingDataSetDocumentsFails_ThenNoDataSetDocumentsRemoved()
    {
        _dataSetDocumentCreatorMockBuilder.WhereThrows(new InvalidOperationException());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            GetSut().RefreshReleaseDataSetDocuments(Message, new FunctionContextMockBuilder().Build())
        );

        _dataSetDocumentRemoverMockBuilder.Assert.RemoveDataSetDocumentsNotCalled();
    }

    [Fact]
    public async Task WhenDataSetDocumentsUploaded_ThenDataSetDocumentsChangedMessageReturned()
    {
        var response = await GetSut().RefreshReleaseDataSetDocuments(Message, new FunctionContextMockBuilder().Build());

        Assert.Equal(
            [
                new DataSetDocumentsChangedMessageDto
                {
                    PublicationId = Message.PublicationId,
                    ReleaseId = Message.ReleaseId,
                },
            ],
            response
        );
    }

    [Fact]
    public async Task WhenDataSetDocumentsRemoved_ThenDataSetDocumentsChangedMessageReturned()
    {
        _dataSetDocumentCreatorMockBuilder.WhereResponseIs(new CreateDataSetDocumentsResponse([]));
        _dataSetDocumentRemoverMockBuilder.WhereResponseIs(new RemoveDataSetDocumentsResponse(["removed-blob"]));

        var response = await GetSut().RefreshReleaseDataSetDocuments(Message, new FunctionContextMockBuilder().Build());

        Assert.Single(response);
    }

    [Fact]
    public async Task WhenNoDataSetDocumentsUploadedOrRemoved_ThenNoMessageReturned()
    {
        _dataSetDocumentCreatorMockBuilder.WhereResponseIs(new CreateDataSetDocumentsResponse([]));

        var response = await GetSut().RefreshReleaseDataSetDocuments(Message, new FunctionContextMockBuilder().Build());

        Assert.Empty(response);
    }

    [Theory]
    [MemberData(nameof(TheoryDatas.Blank.Guids), MemberType = typeof(TheoryDatas.Blank))]
    public async Task WhenMessageHasNoPublicationId_ThenDataSetDocumentsNotRefreshed(Guid? blankPublicationId)
    {
        var response = await GetSut()
            .RefreshReleaseDataSetDocuments(
                Message with
                {
                    PublicationId = blankPublicationId,
                },
                new FunctionContextMockBuilder().Build()
            );

        Assert.Empty(response);
        _dataSetDocumentCreatorMockBuilder.Assert.CreateReleaseDataSetDocumentsNotCalled();
        _dataSetDocumentRemoverMockBuilder.Assert.RemoveDataSetDocumentsNotCalled();
    }

    [Theory]
    [MemberData(nameof(TheoryDatas.Blank.Strings), MemberType = typeof(TheoryDatas.Blank))]
    public async Task WhenMessageHasNoPublicationSlug_ThenDataSetDocumentsNotRefreshed(string? blankPublicationSlug)
    {
        var response = await GetSut()
            .RefreshReleaseDataSetDocuments(
                Message with
                {
                    PublicationSlug = blankPublicationSlug,
                },
                new FunctionContextMockBuilder().Build()
            );

        Assert.Empty(response);
        _dataSetDocumentCreatorMockBuilder.Assert.CreateReleaseDataSetDocumentsNotCalled();
        _dataSetDocumentRemoverMockBuilder.Assert.RemoveDataSetDocumentsNotCalled();
    }

    [Theory]
    [MemberData(nameof(TheoryDatas.Blank.Guids), MemberType = typeof(TheoryDatas.Blank))]
    public async Task WhenMessageHasNoReleaseId_ThenDataSetDocumentsNotRefreshed(Guid? blankReleaseId)
    {
        var response = await GetSut()
            .RefreshReleaseDataSetDocuments(
                Message with
                {
                    ReleaseId = blankReleaseId,
                },
                new FunctionContextMockBuilder().Build()
            );

        Assert.Empty(response);
        _dataSetDocumentCreatorMockBuilder.Assert.CreateReleaseDataSetDocumentsNotCalled();
        _dataSetDocumentRemoverMockBuilder.Assert.RemoveDataSetDocumentsNotCalled();
    }

    [Theory]
    [MemberData(nameof(TheoryDatas.Blank.Strings), MemberType = typeof(TheoryDatas.Blank))]
    public async Task WhenMessageHasNoReleaseSlug_ThenDataSetDocumentsNotRefreshed(string? blankReleaseSlug)
    {
        var response = await GetSut()
            .RefreshReleaseDataSetDocuments(
                Message with
                {
                    ReleaseSlug = blankReleaseSlug,
                },
                new FunctionContextMockBuilder().Build()
            );

        Assert.Empty(response);
        _dataSetDocumentCreatorMockBuilder.Assert.CreateReleaseDataSetDocumentsNotCalled();
        _dataSetDocumentRemoverMockBuilder.Assert.RemoveDataSetDocumentsNotCalled();
    }
}
