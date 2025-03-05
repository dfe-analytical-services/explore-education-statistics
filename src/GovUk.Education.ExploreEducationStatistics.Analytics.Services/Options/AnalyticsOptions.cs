namespace GovUk.Education.ExploreEducationStatistics.Analytics.Service.Options;

public class AnalyticsOptions
{
    public const string Section = "Analytics";

    public bool Enabled { get; init; }

    public string BasePath { get; init; } = string.Empty;
}
