namespace GovUk.Education.ExploreEducationStatistics.Notifier.Configuration;

public class AppSettingsOptions
{
    public const string Section = "AppSettings";

    public string BaseUrl { get; init; } = null!;

    public string PublicAppUrl { get; init; } = null!;

    public string NotifierStorageConnectionString { get; init; } = null!;

    public string TokenSecretKey { get; init; } = null!;
}
