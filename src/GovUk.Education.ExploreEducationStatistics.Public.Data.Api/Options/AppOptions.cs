namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Options;

public class AppOptions
{
    public const string Section = "App";

    /// <summary>
    /// The host URL of the public API.
    /// </summary>
    public string Url { get; init; } = string.Empty;

    public bool EnableSwagger { get; init; }
}
