namespace GovUk.Education.ExploreEducationStatistics.Notifier.Configuration;

public class AppOptions
{
    public const string Section = "App";

    public string Url { get; init; } = null!;

    public string PublicAppUrl { get; init; } = null!;

    public string NotifierStorageConnectionString { get; init; } = null!;

    public string TokenSecretKey { get; init; } = null!;
}
