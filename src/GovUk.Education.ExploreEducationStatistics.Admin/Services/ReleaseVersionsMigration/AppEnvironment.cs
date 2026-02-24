namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.ReleaseVersionsMigration;

/// <summary>
/// TODO EES-6885 Remove after the Release Versions migration is complete.
/// </summary>
public enum AppEnvironment
{
    Local,
    Dev,
    Test,
    PreProd,
    Prod,
}

public static class AppEnvironmentUrls
{
    public const string AppEnvironmentUrlLocal = "https://localhost:5021";
    public const string AppEnvironmentUrlDev = "https://admin.dev.explore-education-statistics.service.gov.uk";
    public const string AppEnvironmentUrlTest = "https://admin.test.explore-education-statistics.service.gov.uk";
    public const string AppEnvironmentUrlPreProd =
        "https://admin.pre-production.explore-education-statistics.service.gov.uk";
    public const string AppEnvironmentUrlProd = "https://admin.explore-education-statistics.service.gov.uk";
}

public static class AppEnvironmentExtensions
{
    public static string GetAppEnvironmentUrl(this AppEnvironment appEnvironment)
    {
        return appEnvironment switch
        {
            AppEnvironment.Local => AppEnvironmentUrls.AppEnvironmentUrlLocal,
            AppEnvironment.Dev => AppEnvironmentUrls.AppEnvironmentUrlDev,
            AppEnvironment.Test => AppEnvironmentUrls.AppEnvironmentUrlTest,
            AppEnvironment.PreProd => AppEnvironmentUrls.AppEnvironmentUrlPreProd,
            AppEnvironment.Prod => AppEnvironmentUrls.AppEnvironmentUrlProd,
            _ => throw new ArgumentOutOfRangeException(nameof(appEnvironment), appEnvironment, null),
        };
    }
}
