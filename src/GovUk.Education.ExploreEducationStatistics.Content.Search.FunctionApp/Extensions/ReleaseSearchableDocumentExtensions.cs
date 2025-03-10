using System.Text;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.ContentApi;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Domain;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Extensions;

/// <summary>
/// The Metadata Keys used to enrich a Searchable Document blob
/// </summary>
public static class SearchableDocumentAzureBlobMetadataKeys
{
    public const string ReleaseVersionId = "RELEASE-VERSION-ID";
    public const string PublicationSlug = "PUBLICATION-SLUG";
    public const string ReleaseSlug = "RELEASE-SLUG";
    public const string Published = "PUBLISHED";
    public const string Summary = "SUMMARY";
    public const string Title = "TITLE";
    public const string Theme = "THEME";
    public const string ReleaseType = "RELEASE-TYPE";
    public const string TypeBoost = "TYPE-BOOST";
}

public static class ReleaseSearchableDocumentExtensions
{
    public static IDictionary<string, string> BuildMetadata(this ReleaseSearchableDocument releaseSearchableDocument)
    {
        var metadata = new Dictionary<string, string>()
        {
            { SearchableDocumentAzureBlobMetadataKeys.ReleaseVersionId, releaseSearchableDocument.ReleaseVersionId.ToString() },
            { SearchableDocumentAzureBlobMetadataKeys.PublicationSlug, releaseSearchableDocument.PublicationSlug },
            { SearchableDocumentAzureBlobMetadataKeys.ReleaseSlug, releaseSearchableDocument.ReleaseSlug },
            { SearchableDocumentAzureBlobMetadataKeys.Published, releaseSearchableDocument.Published.ToUniversalTime().ToString("yyyy-MM-ddThh:mm:ssZ") ?? string.Empty },
            { SearchableDocumentAzureBlobMetadataKeys.Summary, ToMetadataSafeString(releaseSearchableDocument.Summary) },
            { SearchableDocumentAzureBlobMetadataKeys.Title, ToMetadataSafeString(releaseSearchableDocument.PublicationTitle) },
            { SearchableDocumentAzureBlobMetadataKeys.Theme, releaseSearchableDocument.Theme },
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
