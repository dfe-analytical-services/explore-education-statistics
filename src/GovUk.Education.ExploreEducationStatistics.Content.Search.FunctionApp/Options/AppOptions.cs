using System.Configuration;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Options;

public class AppOptions
{
    public const string Section = "App";
    public string SearchStorageConnectionString { get; init; } = string.Empty;
    public string SearchableDocumentsContainerName { get; init; } = string.Empty;
    
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(SearchStorageConnectionString))
        {
            throw new ConfigurationErrorsException($"Azure Search Storage Connection String is not configured. Ensure the {Section}:{nameof(SearchStorageConnectionString)} is set.");
        }
        if (string.IsNullOrWhiteSpace(SearchableDocumentsContainerName))
        {
            throw new ConfigurationErrorsException($"Azure Search Storage Container Name is not configured. Ensure the {Section}:{nameof(SearchableDocumentsContainerName)} is set.");
        }
    }
}
