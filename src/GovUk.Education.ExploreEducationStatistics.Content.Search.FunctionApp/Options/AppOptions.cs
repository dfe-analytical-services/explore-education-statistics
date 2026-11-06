using System.Diagnostics.CodeAnalysis;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Options;

public class AppOptions
{
    public const string Section = "App";
    public string SearchStorageConnectionString { get; init; } = string.Empty;
    public string ReleaseDocumentsContainerName { get; init; } = string.Empty;
    public string DatasetDocumentsContainerName { get; init; } = string.Empty;

    public bool IsValid([NotNullWhen(false)] out string? errorMessage)
    {
        errorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(SearchStorageConnectionString))
        {
            errorMessage +=
                $"Search Azure Storage account connection string is not configured. Ensure the {Section}:{nameof(SearchStorageConnectionString)} is set. ";
        }
        if (string.IsNullOrWhiteSpace(ReleaseDocumentsContainerName))
        {
            errorMessage +=
                $"Release Documents Azure Blob Storage container name is not configured. Ensure the {Section}:{nameof(ReleaseDocumentsContainerName)} is set. ";
        }
        if (string.IsNullOrWhiteSpace(DatasetDocumentsContainerName))
        {
            errorMessage +=
                $"Dataset Documents Azure Blob Storage container name is not configured. Ensure the {Section}:{nameof(DatasetDocumentsContainerName)} is set. ";
        }

        errorMessage = errorMessage.TrimEnd();

        return errorMessage == string.Empty;
    }
}
