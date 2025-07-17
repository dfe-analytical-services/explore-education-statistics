using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Domain;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Extensions;

/// <summary>
/// The Metadata Keys used to enrich a Searchable Document blob
/// </summary>
public static class SearchableDocumentAzureBlobMetadataKeys
{
    public const string ReleaseId = "ReleaseId";
    public const string ReleaseSlug = "ReleaseSlug";
    public const string ReleaseVersionId = "ReleaseVersionId";
    public const string PublicationId = "PublicationId";
    public const string PublicationSlug = "PublicationSlug";
    public const string ThemeId = "ThemeId";
    public const string ThemeTitle = "ThemeTitle";
    public const string Published = "Published";
    public const string Summary = "Summary";
    public const string Title = "Title";
    public const string ReleaseType = "ReleaseType";
    public const string TypeBoost = "TypeBoost";
}

public static class ReleaseSearchableDocumentExtensions
{
    public static IDictionary<string, string> BuildMetadata(this ReleaseSearchableDocument releaseSearchableDocument)
    {
        // Metadata key/value pairs are set using HTTP headers and must be valid headers containing only ASCII characters.
        // Values are Base64 encoded if non-ASCII characters might be present.
        var metadata = new Dictionary<string, string>
        {
            { SearchableDocumentAzureBlobMetadataKeys.ReleaseId, releaseSearchableDocument.ReleaseId.ToString() },
            {
                SearchableDocumentAzureBlobMetadataKeys.ReleaseSlug,
                releaseSearchableDocument.ReleaseSlug.ToBase64String()
            },
            {
                SearchableDocumentAzureBlobMetadataKeys.ReleaseVersionId,
                releaseSearchableDocument.ReleaseVersionId.ToString()
            },
            {
                SearchableDocumentAzureBlobMetadataKeys.PublicationId,
                releaseSearchableDocument.PublicationId.ToString()
            },
            {
                SearchableDocumentAzureBlobMetadataKeys.PublicationSlug,
                releaseSearchableDocument.PublicationSlug.ToBase64String()
            },
            { SearchableDocumentAzureBlobMetadataKeys.ThemeId, releaseSearchableDocument.ThemeId.ToString() },
            {
                SearchableDocumentAzureBlobMetadataKeys.ThemeTitle,
                releaseSearchableDocument.ThemeTitle.TrimAndBase64Encode()
            },
            {
                SearchableDocumentAzureBlobMetadataKeys.Published,
                releaseSearchableDocument.Published.ToUniversalTime().ToString("yyyy-MM-ddThh:mm:ssZ")
            },
            {
                SearchableDocumentAzureBlobMetadataKeys.Summary, releaseSearchableDocument.Summary.TrimAndBase64Encode()
            },
            {
                SearchableDocumentAzureBlobMetadataKeys.Title,
                releaseSearchableDocument.PublicationTitle.TrimAndBase64Encode()
            },
            { SearchableDocumentAzureBlobMetadataKeys.ReleaseType, releaseSearchableDocument.ReleaseType },
            { SearchableDocumentAzureBlobMetadataKeys.TypeBoost, releaseSearchableDocument.TypeBoost.ToString() },
        };
        return metadata;
    }
}
