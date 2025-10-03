namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.CreateSearchableDocuments;

public interface ISearchableDocumentCreator
{
    Task<CreatePublicationLatestReleaseSearchableDocumentResponse> CreatePublicationLatestReleaseSearchableDocument(
        CreatePublicationLatestReleaseSearchableDocumentRequest request,
        CancellationToken cancellationToken = default
    );
}
