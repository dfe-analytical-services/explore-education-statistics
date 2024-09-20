namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Options;

public class AppSettingsOptions
{
    public const string Section = "AppSettings";

    /// <summary>
    /// The host URL of the public API.
    /// </summary>
    public string HostUrl { get; init; } = string.Empty;
}
