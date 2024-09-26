namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Options;

public class AppOptions
{
    public const string Section = "App";

    /// <summary>
    /// The host URL of the public API.
    /// </summary>
    public string HostUrl { get; init; } = string.Empty;
}
