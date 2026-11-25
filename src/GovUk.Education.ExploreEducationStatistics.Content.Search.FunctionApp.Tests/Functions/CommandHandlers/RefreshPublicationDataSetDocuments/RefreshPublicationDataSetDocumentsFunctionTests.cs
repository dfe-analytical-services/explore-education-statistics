using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Domain;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.RefreshPublicationDataSetDocuments;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.RefreshPublicationDataSetDocuments.Dto;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.ReindexDataSetDocuments.Dto;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.CreateDataSetDocuments;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.RemoveDataSetDocuments;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Builders;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.TheoryDataHelpers;
using Microsoft.Extensions.Logging.Abstractions;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Functions.CommandHandlers.RefreshPublicationDataSetDocuments;

public class RefreshPublicationDataSetDocumentsFunctionTests
{
    private static readonly RefreshPublicationDataSetDocumentsMessageDto Message = new()
    {
        PublicationId = Guid.NewGuid(),
        PublicationSlug = "publication-slug",
    };

    private static readonly DataSetDocumentKey CreatedKey = new()
    {
        PublicationId = Message.PublicationId!.Value,
        ReleaseId = Guid.NewGuid(),
        FileId = Guid.NewGuid(),
    };

    private readonly DataSetDocumentCreatorMockBuilder _dataSetDocumentCreatorMockBuilder = new();
    private readonly DataSetDocumentRemoverMockBuilder _dataSetDocumentRemoverMockBuilder = new();

    public RefreshPublicationDataSetDocumentsFunctionTests()
    {
        _dataSetDocumentCreatorMockBuilder.WhereResponseIs(new CreateDataSetDocumentsResponse([CreatedKey]));
    }

    private RefreshPublicationDataSetDocumentsFunction GetSut() =>
        new(
            new NullLogger<RefreshPublicationDataSetDocumentsFunction>(),
            _dataSetDocumentCreatorMockBuilder.Build(),
            _dataSetDocumentRemoverMockBuilder.Build(),
            new TestableCommandHandler()
        );

    [Fact]
    public void CanInstantiateSut() => Assert.NotNull(GetSut());

    [Fact]
    public async Task WhenMessageIdentifiesPublication_ThenDataSetDocumentsOfPublicationCreated()
    {
        await GetSut().RefreshPublicationDataSetDocuments(Message, new FunctionContextMockBuilder().Build());

        _dataSetDocumentCreatorMockBuilder.Assert.CreatePublicationDataSetDocumentsCalledFor(
            new CreatePublicationDataSetDocumentsRequest
            {
                PublicationId = Message.PublicationId!.Value,
                PublicationSlug = "publication-slug",
            }
        );
    }

    [Fact]
    public async Task WhenMessageIdentifiesPublication_ThenOtherDataSetDocumentsOfPublicationRemoved()
    {
        await GetSut().RefreshPublicationDataSetDocuments(Message, new FunctionContextMockBuilder().Build());

        _dataSetDocumentRemoverMockBuilder.Assert.RemoveDataSetDocumentsCalledFor(request =>
            request.PublicationId == Message.PublicationId
            && request.ReleaseId == null
            && request.FileId == null
            && request.Except.SequenceEqual([CreatedKey])
        );
    }

    [Fact]
    public async Task WhenPublicationHasNoDataSetDocuments_ThenAllDataSetDocumentsOfPublicationRemoved()
    {
        _dataSetDocumentCreatorMockBuilder.WhereResponseIs(new CreateDataSetDocumentsResponse([]));

        await GetSut().RefreshPublicationDataSetDocuments(Message, new FunctionContextMockBuilder().Build());

        _dataSetDocumentRemoverMockBuilder.Assert.RemoveDataSetDocumentsCalledFor(request =>
            request.PublicationId == Message.PublicationId
            && request.ReleaseId == null
            && request.FileId == null
            && request.Except.Count == 0
        );
    }

    [Fact]
    public async Task WhenCreatingDataSetDocumentsFails_ThenNoDataSetDocumentsRemoved()
    {
        _dataSetDocumentCreatorMockBuilder.WhereThrows(new InvalidOperationException());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            GetSut().RefreshPublicationDataSetDocuments(Message, new FunctionContextMockBuilder().Build())
        );

        _dataSetDocumentRemoverMockBuilder.Assert.RemoveDataSetDocumentsNotCalled();
    }

    [Fact]
    public async Task WhenDataSetDocumentsUploaded_ThenDataSetDocumentsChangedMessageReturned()
    {
        var response = await GetSut()
            .RefreshPublicationDataSetDocuments(Message, new FunctionContextMockBuilder().Build());

        Assert.Equal([new DataSetDocumentsChangedMessageDto { PublicationId = Message.PublicationId }], response);
    }

    [Fact]
    public async Task WhenDataSetDocumentsRemoved_ThenDataSetDocumentsChangedMessageReturned()
    {
        _dataSetDocumentCreatorMockBuilder.WhereResponseIs(new CreateDataSetDocumentsResponse([]));
        _dataSetDocumentRemoverMockBuilder.WhereResponseIs(new RemoveDataSetDocumentsResponse(["removed-blob"]));

        var response = await GetSut()
            .RefreshPublicationDataSetDocuments(Message, new FunctionContextMockBuilder().Build());

        Assert.Single(response);
    }

    [Fact]
    public async Task WhenNoDataSetDocumentsUploadedOrRemoved_ThenNoMessageReturned()
    {
        _dataSetDocumentCreatorMockBuilder.WhereResponseIs(new CreateDataSetDocumentsResponse([]));

        var response = await GetSut()
            .RefreshPublicationDataSetDocuments(Message, new FunctionContextMockBuilder().Build());

        Assert.Empty(response);
    }

    [Theory]
    [MemberData(nameof(TheoryDatas.Blank.Guids), MemberType = typeof(TheoryDatas.Blank))]
    public async Task WhenMessageHasNoPublicationId_ThenDataSetDocumentsNotRefreshed(Guid? blankPublicationId)
    {
        var response = await GetSut()
            .RefreshPublicationDataSetDocuments(
                Message with
                {
                    PublicationId = blankPublicationId,
                },
                new FunctionContextMockBuilder().Build()
            );

        Assert.Empty(response);
        _dataSetDocumentCreatorMockBuilder.Assert.CreatePublicationDataSetDocumentsNotCalled();
        _dataSetDocumentRemoverMockBuilder.Assert.RemoveDataSetDocumentsNotCalled();
    }

    [Theory]
    [MemberData(nameof(TheoryDatas.Blank.Strings), MemberType = typeof(TheoryDatas.Blank))]
    public async Task WhenMessageHasNoPublicationSlug_ThenDataSetDocumentsNotRefreshed(string? blankPublicationSlug)
    {
        var response = await GetSut()
            .RefreshPublicationDataSetDocuments(
                Message with
                {
                    PublicationSlug = blankPublicationSlug,
                },
                new FunctionContextMockBuilder().Build()
            );

        Assert.Empty(response);
        _dataSetDocumentCreatorMockBuilder.Assert.CreatePublicationDataSetDocumentsNotCalled();
        _dataSetDocumentRemoverMockBuilder.Assert.RemoveDataSetDocumentsNotCalled();
    }
}
