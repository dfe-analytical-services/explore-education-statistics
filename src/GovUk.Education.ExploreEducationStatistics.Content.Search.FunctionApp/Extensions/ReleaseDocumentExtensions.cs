using System.Text.Json;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Domain;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Extensions;

/// <summary>
/// The Metadata Keys used to enrich a Searchable Document blob
/// </summary>
public static class ReleaseDocumentAzureBlobMetadataKeys
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
    public const string PublishingOrganisationIds = "PublishingOrganisationIds";
    public const string PublishingOrganisationTitles = "PublishingOrganisationTitles";
}

public static class ReleaseDocumentExtensions
{
    public static IDictionary<string, string> BuildMetadata(this ReleaseDocument releaseDocument)
    {
        // Metadata key/value pairs are set using HTTP headers and must be valid headers containing only ASCII characters.
        // Values are Base64 encoded if non-ASCII characters might be present.
        // Collection values are serialised as JSON string arrays.
        var metadata = new Dictionary<string, string>
        {
            { ReleaseDocumentAzureBlobMetadataKeys.ReleaseId, releaseDocument.ReleaseId.ToString() },
            { ReleaseDocumentAzureBlobMetadataKeys.ReleaseSlug, releaseDocument.ReleaseSlug.ToBase64String() },
            { ReleaseDocumentAzureBlobMetadataKeys.ReleaseVersionId, releaseDocument.ReleaseVersionId.ToString() },
            { ReleaseDocumentAzureBlobMetadataKeys.PublicationId, releaseDocument.PublicationId.ToString() },
            { ReleaseDocumentAzureBlobMetadataKeys.PublicationSlug, releaseDocument.PublicationSlug.ToBase64String() },
            { ReleaseDocumentAzureBlobMetadataKeys.ThemeId, releaseDocument.ThemeId.ToString() },
            { ReleaseDocumentAzureBlobMetadataKeys.ThemeTitle, releaseDocument.ThemeTitle.TrimAndBase64Encode() },
            {
                ReleaseDocumentAzureBlobMetadataKeys.Published,
                releaseDocument.Published.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ")
            },
            { ReleaseDocumentAzureBlobMetadataKeys.Summary, releaseDocument.Summary.TrimAndBase64Encode() },
            { ReleaseDocumentAzureBlobMetadataKeys.Title, releaseDocument.PublicationTitle.TrimAndBase64Encode() },
            { ReleaseDocumentAzureBlobMetadataKeys.ReleaseType, releaseDocument.ReleaseType },
            { ReleaseDocumentAzureBlobMetadataKeys.TypeBoost, releaseDocument.TypeBoost.ToString() },
            {
                ReleaseDocumentAzureBlobMetadataKeys.PublishingOrganisationIds,
                JsonSerializer.Serialize(
                    releaseDocument
                        .PublishingOrganisations.Select(publishingOrganisation => publishingOrganisation.Id)
                        .ToArray()
                )
            },
            {
                ReleaseDocumentAzureBlobMetadataKeys.PublishingOrganisationTitles,
                JsonSerializer.Serialize(
                    releaseDocument
                        .PublishingOrganisations.Select(publishingOrganisation => publishingOrganisation.Title)
                        .ToArray()
                )
            },
        };
        return metadata;
    }
}
