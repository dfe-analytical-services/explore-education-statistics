using System.Text;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.ContentApi;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Domain;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Extensions;

/// <summary>
/// The Metadata Keys used to enrich a Searchable Document blob
/// </summary>
public static class SearchableDocumentAzureBlobMetadataKeys
{
    public const string ReleaseVersionId = "releaseVersionId";
    public const string PublicationSlug = "publicationSlug";
    public const string ReleaseSlug = "releaseSlug";
    public const string Published = "published";
    public const string Summary = "summary";
    public const string Title = "title";
    public const string Theme = "theme";
    public const string Type = "type";
    public const string TypeBoost = "typeBoost";
}
public static class ReleaseSearchViewModelExtensions
{
    public static IDictionary<string, string> BuildMetadata(this ReleaseSearchableDocument searchViewModel)
    {
        var metadata = new Dictionary<string, string>()
        {
            { SearchableDocumentAzureBlobMetadataKeys.ReleaseVersionId, searchViewModel.ReleaseVersionId.ToString() },
            { SearchableDocumentAzureBlobMetadataKeys.PublicationSlug, searchViewModel.PublicationSlug },
            { SearchableDocumentAzureBlobMetadataKeys.ReleaseSlug, searchViewModel.ReleaseSlug },
            { SearchableDocumentAzureBlobMetadataKeys.Published, searchViewModel.Published.ToUniversalTime().ToString("yyyy-MM-ddThh:mm:ssZ") ?? string.Empty },
            { SearchableDocumentAzureBlobMetadataKeys.Summary, ToMetadataSafeString(searchViewModel.Summary) },
            { SearchableDocumentAzureBlobMetadataKeys.Title, ToMetadataSafeString(searchViewModel.PublicationTitle) },
            { SearchableDocumentAzureBlobMetadataKeys.Theme, searchViewModel.Theme },
            { SearchableDocumentAzureBlobMetadataKeys.Type, searchViewModel.Type },
            { SearchableDocumentAzureBlobMetadataKeys.TypeBoost, searchViewModel.TypeBoost.ToString() },
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
