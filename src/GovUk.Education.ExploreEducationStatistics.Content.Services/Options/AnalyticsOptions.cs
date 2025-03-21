namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Options;

public class AnalyticsOptions
{
    public const string Section = "Analytics";

    public bool Enabled { get; init; } = false;

    public string BasePath { get; init; } = string.Empty;
}
