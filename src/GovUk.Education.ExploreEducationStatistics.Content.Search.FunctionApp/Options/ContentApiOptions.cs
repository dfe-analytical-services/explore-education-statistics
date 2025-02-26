using System.Configuration;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Options;

public class ContentApiOptions
{
    public const string Section = "ContentApi";

    public string Url { get; init; } = string.Empty;

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Url))
        {
            throw new ConfigurationErrorsException($"ContentApi base address is not configured. Ensure the {Section}:{nameof(Url)} is set.");
        }
    }
}
