#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Admin.Options;

public class AppInsightsOptions
{
    public const string Section = "AppInsights";

    public string InstrumentationKey { get; set; } = string.Empty;
}
