namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Options;

public class ApplicationInsightsOptions
{
    public const string Section = "ApplicationInsights";

    public string ConnectionString { get; init; } = string.Empty;
}
