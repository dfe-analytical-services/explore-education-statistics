using System.Diagnostics.CodeAnalysis;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Options;

public class ContentApiOptions
{
    public const string Section = "ContentApi";

    public string Url { get; init; } = string.Empty;

    public bool IsValid([NotNullWhen(false)] out string? errorMessage)
    {
        if (string.IsNullOrWhiteSpace(Url))
        {
            errorMessage =
                $"ContentApi base address is not configured. Ensure the {Section}:{nameof(Url)} is set.";
            return false;
        }

        errorMessage = null;
        return true;
    }
}
