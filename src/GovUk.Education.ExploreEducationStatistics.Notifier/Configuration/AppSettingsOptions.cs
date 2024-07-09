namespace GovUk.Education.ExploreEducationStatistics.Notifier.Configuration;

public class AppSettingsOptions
{
    public const string AppSettings = "AppSettings";

    public string BaseUrl { get; init; } = null!;

    public string PublicAppUrl { get; init; } = null!;

    public string TableStorageConnectionString { get; init; } = null!;

    public string TokenSecretKey { get; init; } = null!;

    public string ApiSubscriptionsTableName { get; init; } = null!;
}
