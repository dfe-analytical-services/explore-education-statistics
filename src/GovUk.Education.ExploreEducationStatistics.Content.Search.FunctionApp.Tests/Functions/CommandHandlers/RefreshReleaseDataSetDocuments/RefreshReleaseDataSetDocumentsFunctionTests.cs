using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.RefreshReleaseDataSetDocuments;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.RefreshReleaseDataSetDocuments.Dto;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.ReindexDataSetDocuments.Dto;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.CreateDataSetDocuments;
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

    private readonly DataSetDocumentCreatorMockBuilder _dataSetDocumentCreatorMockBuilder = new();

    public RefreshReleaseDataSetDocumentsFunctionTests()
    {
        _dataSetDocumentCreatorMockBuilder.WhereResponseIs(new RefreshDataSetDocumentsResponse(["uploaded-blob"], []));
    }

    private RefreshReleaseDataSetDocumentsFunction GetSut() =>
        new(
            new NullLogger<RefreshReleaseDataSetDocumentsFunction>(),
            _dataSetDocumentCreatorMockBuilder.Build(),
            new TestableCommandHandler()
        );

    [Fact]
    public void CanInstantiateSut() => Assert.NotNull(GetSut());

    [Fact]
    public async Task WhenMessageIdentifiesRelease_ThenDataSetDocumentsOfReleaseRefreshed()
    {
        await GetSut().RefreshReleaseDataSetDocuments(Message, new FunctionContextMockBuilder().Build());

        _dataSetDocumentCreatorMockBuilder.Assert.RefreshReleaseDataSetDocumentsCalledFor(
            new RefreshReleaseDataSetDocumentsRequest
            {
                PublicationId = Message.PublicationId!.Value,
                PublicationSlug = "publication-slug",
                ReleaseId = Message.ReleaseId!.Value,
                ReleaseSlug = "release-slug",
            }
        );
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
        _dataSetDocumentCreatorMockBuilder.WhereResponseIs(new RefreshDataSetDocumentsResponse([], ["removed-blob"]));

        var response = await GetSut().RefreshReleaseDataSetDocuments(Message, new FunctionContextMockBuilder().Build());

        Assert.Single(response);
    }

    [Fact]
    public async Task WhenNoDataSetDocumentsUploadedOrRemoved_ThenNoMessageReturned()
    {
        _dataSetDocumentCreatorMockBuilder.WhereResponseIs(new RefreshDataSetDocumentsResponse([], []));

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
        _dataSetDocumentCreatorMockBuilder.Assert.RefreshReleaseDataSetDocumentsNotCalled();
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
        _dataSetDocumentCreatorMockBuilder.Assert.RefreshReleaseDataSetDocumentsNotCalled();
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
        _dataSetDocumentCreatorMockBuilder.Assert.RefreshReleaseDataSetDocumentsNotCalled();
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
        _dataSetDocumentCreatorMockBuilder.Assert.RefreshReleaseDataSetDocumentsNotCalled();
    }
}
