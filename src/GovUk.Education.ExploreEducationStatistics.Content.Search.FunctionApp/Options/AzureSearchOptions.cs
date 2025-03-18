using System.Diagnostics.CodeAnalysis;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Options;

public class AzureSearchOptions
{
    public const string Section = "AzureSearch";
    
    /// <summary>
    /// Url of the search service endpoint. Likely to be similar to:
    /// https://{search_service}.search.windows.net
    /// </summary>
    public string SearchServiceEndpoint { get; init; } = string.Empty;
    public string? SearchServiceAccessKey { get; init; }
    
    public string IndexName { get; init; } = string.Empty;
    
    public bool IsValid([NotNullWhen(false)] out string? errorMessage)
    {
        if (string.IsNullOrWhiteSpace(SearchServiceEndpoint))
        {
            errorMessage = $"Azure Search Endpoint is not configured. Ensure the {Section}:{nameof(SearchServiceEndpoint)} is set.";
            return false;
        }
                
        if (string.IsNullOrWhiteSpace(IndexName))
        {
            errorMessage = $"Azure Search Index Name is not configured. Ensure the {Section}:{nameof(IndexName)} is set.";
            return false;
        }
        
        errorMessage = null;
        return true;
    }
}
