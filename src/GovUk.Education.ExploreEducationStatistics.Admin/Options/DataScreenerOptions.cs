namespace GovUk.Education.ExploreEducationStatistics.Admin.Options;

public class DataScreenerOptions : IAzureAuthenticationOptions
{
    public const string Section = "DataScreener";

    public string Url { get; init; }

    public Guid AppRegistrationClientId { get; init; }

    public string ScreenerStorage { get; init; }

    public bool EnhancedScreenerJourney { get; init; }

    public int ScreenerProgressUpdateIntervalSeconds { get; init; }
}
