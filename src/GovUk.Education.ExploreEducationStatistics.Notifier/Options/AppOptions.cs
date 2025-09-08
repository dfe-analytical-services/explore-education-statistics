namespace GovUk.Education.ExploreEducationStatistics.Notifier.Options;

public class AppOptions
{
    public const string Section = "App";

    public bool EmailEnabled { get; init; }

    public bool SuppressExceptionsForTeamOnlyApiKeyErrors { get; init; }

    public string Url { get; init; } = null!;

    public string PublicAppUrl { get; init; } = null!;

    public string NotifierStorageConnectionString { get; init; } = null!;

    public string TokenSecretKey { get; init; } = null!;
}
