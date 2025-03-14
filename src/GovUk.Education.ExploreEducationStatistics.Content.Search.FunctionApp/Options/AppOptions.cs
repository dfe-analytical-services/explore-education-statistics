using System.Diagnostics.CodeAnalysis;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Options;

public class AppOptions
{
    public const string Section = "App";
    public string SearchStorageConnectionString { get; init; } = string.Empty;
    public string SearchableDocumentsContainerName { get; init; } = string.Empty;
    
    public bool IsValid([NotNullWhen(false)] out string? errorMessage)
    {
        errorMessage = string.Empty;
        
        if (string.IsNullOrWhiteSpace(SearchStorageConnectionString))
        {
            errorMessage += $"Azure Search Storage Connection String is not configured. Ensure the {Section}:{nameof(SearchStorageConnectionString)} is set. ";
        }
        if (string.IsNullOrWhiteSpace(SearchableDocumentsContainerName))
        {
            errorMessage += $"Azure Search Storage Container Name is not configured. Ensure the {Section}:{nameof(SearchableDocumentsContainerName)} is set.";
        }
        
        return errorMessage == string.Empty;
    }
}
