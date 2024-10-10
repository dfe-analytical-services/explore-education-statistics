#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Admin.Options;

public class PublicAppOptions
{
    public const string Section = "PublicApp";

    /// <summary>
    /// The URL of the public frontend app.
    /// </summary>
    public string Url { get; init; } = string.Empty;
}
