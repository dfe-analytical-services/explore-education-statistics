using System.Text;
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
        var metadata = new Dictionary<string, string>
        {
            { SearchableDocumentAzureBlobMetadataKeys.ReleaseId, releaseSearchableDocument.ReleaseId.ToString() },
            { SearchableDocumentAzureBlobMetadataKeys.ReleaseSlug, releaseSearchableDocument.ReleaseSlug },
            { SearchableDocumentAzureBlobMetadataKeys.ReleaseVersionId, releaseSearchableDocument.ReleaseVersionId.ToString() },
            { SearchableDocumentAzureBlobMetadataKeys.PublicationId, releaseSearchableDocument.PublicationId.ToString() },
            { SearchableDocumentAzureBlobMetadataKeys.PublicationSlug, releaseSearchableDocument.PublicationSlug },
            { SearchableDocumentAzureBlobMetadataKeys.ThemeId, releaseSearchableDocument.ThemeId.ToString() },
            { SearchableDocumentAzureBlobMetadataKeys.ThemeTitle, ToMetadataSafeString(releaseSearchableDocument.ThemeTitle) },
            { SearchableDocumentAzureBlobMetadataKeys.Published, releaseSearchableDocument.Published.ToUniversalTime().ToString("yyyy-MM-ddThh:mm:ssZ") },
            { SearchableDocumentAzureBlobMetadataKeys.Summary, ToMetadataSafeString(releaseSearchableDocument.Summary) },
            { SearchableDocumentAzureBlobMetadataKeys.Title, ToMetadataSafeString(releaseSearchableDocument.PublicationTitle) },
            { SearchableDocumentAzureBlobMetadataKeys.ReleaseType, releaseSearchableDocument.ReleaseType },
            { SearchableDocumentAzureBlobMetadataKeys.TypeBoost, releaseSearchableDocument.TypeBoost.ToString() },
        };
        return metadata;
    }

    /// <summary>
    /// Azure metadata must consist of Ascii characters only and have no preceding or trailing whitespace.  
    /// </summary>
    private static string ToMetadataSafeString(object? obj)
    {
        var s = Convert.ToString(obj) ?? string.Empty;
        return EncodeNonAsciiCharacters(s).Trim();
    }
    
    /// <summary>
    /// Encode any non-ascii characters as their Unicode number e.g. \u1234  
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    private static string EncodeNonAsciiCharacters(string value) 
    {
        var sb = new StringBuilder();  
        foreach (var c in value.Normalize(NormalizationForm.FormD).Where(c => !char.IsWhiteSpace(c) || c == ' '))
        {
            if (c <= 127)
            {
                sb.Append(c);
            }
            else
            {
                // This character is too big for ASCII  
                sb.Append("\\u" + ((int)c).ToString("x4"));
            }
        }  

        return sb.ToString();  
    }
}
