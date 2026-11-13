using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.RefreshPublicationDataSetDocuments;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.RefreshPublicationDataSetDocuments.Dto;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.ReindexDataSetDocuments.Dto;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.CreateDataSetDocuments;
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

    private readonly DataSetDocumentCreatorMockBuilder _dataSetDocumentCreatorMockBuilder = new();

    public RefreshPublicationDataSetDocumentsFunctionTests()
    {
        _dataSetDocumentCreatorMockBuilder.WhereResponseIs(new RefreshDataSetDocumentsResponse(["uploaded-blob"], []));
    }

    private RefreshPublicationDataSetDocumentsFunction GetSut() =>
        new(
            new NullLogger<RefreshPublicationDataSetDocumentsFunction>(),
            _dataSetDocumentCreatorMockBuilder.Build(),
            new TestableCommandHandler()
        );

    [Fact]
    public void CanInstantiateSut() => Assert.NotNull(GetSut());

    [Fact]
    public async Task WhenMessageIdentifiesPublication_ThenDataSetDocumentsOfPublicationRefreshed()
    {
        await GetSut().RefreshPublicationDataSetDocuments(Message, new FunctionContextMockBuilder().Build());

        _dataSetDocumentCreatorMockBuilder.Assert.RefreshPublicationDataSetDocumentsCalledFor(
            new RefreshPublicationDataSetDocumentsRequest
            {
                PublicationId = Message.PublicationId!.Value,
                PublicationSlug = "publication-slug",
            }
        );
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
        _dataSetDocumentCreatorMockBuilder.WhereResponseIs(new RefreshDataSetDocumentsResponse([], ["removed-blob"]));

        var response = await GetSut()
            .RefreshPublicationDataSetDocuments(Message, new FunctionContextMockBuilder().Build());

        Assert.Single(response);
    }

    [Fact]
    public async Task WhenNoDataSetDocumentsUploadedOrRemoved_ThenNoMessageReturned()
    {
        _dataSetDocumentCreatorMockBuilder.WhereResponseIs(new RefreshDataSetDocumentsResponse([], []));

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
        _dataSetDocumentCreatorMockBuilder.Assert.RefreshPublicationDataSetDocumentsNotCalled();
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
        _dataSetDocumentCreatorMockBuilder.Assert.RefreshPublicationDataSetDocumentsNotCalled();
    }
}
