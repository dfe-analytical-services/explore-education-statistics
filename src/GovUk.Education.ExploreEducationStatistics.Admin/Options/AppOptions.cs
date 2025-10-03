#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Admin.Options;

public class AppOptions
{
    public const string Section = "App";

    public string Url { get; init; } = string.Empty;

    public bool EnableSwagger { get; init; }

    public bool EnableThemeDeletion { get; init; }

    public bool EnableEinPublishedPageDeletion { get; init; }
}
