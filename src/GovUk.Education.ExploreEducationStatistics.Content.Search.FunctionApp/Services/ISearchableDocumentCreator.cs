namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services;

public interface ISearchableDocumentCreator
{
    Task<CreatePublicationLatestReleaseSearchableDocumentResponse> CreatePublicationLatestReleaseSearchableDocument(
        CreatePublicationLatestReleaseSearchableDocumentRequest request,
        CancellationToken cancellationToken = default);
}
