using System.Diagnostics.CodeAnalysis;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Options;

public class AppOptions
{
    public const string Section = "App";
    public string SearchStorageConnectionString { get; init; } = string.Empty;
    public string SearchDocumentsContainerName { get; init; } = string.Empty;

    public bool IsValid([NotNullWhen(false)] out string? errorMessage)
    {
        errorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(SearchStorageConnectionString))
        {
            errorMessage +=
                $"Search Azure Storage account connection string is not configured. Ensure the {Section}:{nameof(SearchStorageConnectionString)} is set. ";
        }
        if (string.IsNullOrWhiteSpace(SearchDocumentsContainerName))
        {
            errorMessage +=
                $"Search Documents Azure Blob Storage container name is not configured. Ensure the {Section}:{nameof(SearchDocumentsContainerName)} is set.";
        }

        return errorMessage == string.Empty;
    }
}
