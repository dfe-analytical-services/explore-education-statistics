namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.CreateReleaseDocuments;

public interface IReleaseDocumentCreator
{
    Task<CreatePublicationLatestReleaseDocumentResponse> CreatePublicationLatestReleaseDocument(
        CreatePublicationLatestReleaseDocumentRequest request,
        CancellationToken cancellationToken = default
    );
}
