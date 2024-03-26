namespace GovUk.Education.ExploreEducationStatistics.Notifier.Configuration;

public class AppSettingOptions
{
    public const string AppSettings = "AppSettings";

    public string BaseUrl { get; init; } = null!;

    public string PublicAppUrl { get; init; } = null!;

    public string TableStorageConnectionString { get; init; } = null!;

    public string TokenSecretKey { get; init; } = null!;
}
