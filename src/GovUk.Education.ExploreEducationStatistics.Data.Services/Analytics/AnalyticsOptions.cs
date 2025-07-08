#nullable enable
using Microsoft.Extensions.Configuration;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Analytics;

public class AnalyticsOptions
{
    public const string Section = "Analytics";

    public bool Enabled { get; init; } = false;

    public string BasePath { get; init; } = string.Empty;

    public static bool IsEnabled(IConfiguration configuration) =>
        configuration
            .GetSection(Section)
            .Get<AnalyticsOptions>()?
            .Enabled == true;
}
