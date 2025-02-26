using System.Text;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.ContentApi;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Extensions;

public static class ReleaseSearchViewModelAzureBlobMetadataKeys
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
    public static IDictionary<string, string> BuildMetadata(this ReleaseSearchViewModelDto searchViewModel)
    {
        var metadata = new Dictionary<string, string>()
        {
            { ReleaseSearchViewModelAzureBlobMetadataKeys.ReleaseVersionId, searchViewModel.ReleaseVersionId.ToString() },
            { ReleaseSearchViewModelAzureBlobMetadataKeys.PublicationSlug, searchViewModel.PublicationSlug },
            { ReleaseSearchViewModelAzureBlobMetadataKeys.ReleaseSlug, searchViewModel.ReleaseSlug },
            { ReleaseSearchViewModelAzureBlobMetadataKeys.Published, searchViewModel.Published.ToUniversalTime().ToString("yyyy-MM-ddThh:mm:ssZ") ?? string.Empty },
            { ReleaseSearchViewModelAzureBlobMetadataKeys.Summary, ToSafeString(searchViewModel.Summary) },
            { ReleaseSearchViewModelAzureBlobMetadataKeys.Title, ToSafeString(searchViewModel.PublicationTitle) },
            { ReleaseSearchViewModelAzureBlobMetadataKeys.Theme, searchViewModel.Theme },
            { ReleaseSearchViewModelAzureBlobMetadataKeys.Type, searchViewModel.Type },
            { ReleaseSearchViewModelAzureBlobMetadataKeys.TypeBoost, searchViewModel.TypeBoost.ToString() },
        };
        return metadata;
    }

    private static string ToSafeString(object? obj)
    {
        var s = Convert.ToString(obj) ?? string.Empty;
        return EncodeNonAsciiCharacters(s).Trim();
    }
    
    private static string EncodeNonAsciiCharacters(string value ) 
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
