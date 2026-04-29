namespace GovUk.Education.ExploreEducationStatistics.Admin.Options;

public class DataScreenerOptions : IAzureAuthenticationOptions
{
    public const string Section = "DataScreener";

    public string Url { get; init; }

    public Guid AppRegistrationClientId { get; init; }

    public string ScreenerStorage { get; init; }

    public bool EnhancedScreenerJourney { get; init; }

    public int ScreenerProgressUpdateIntervalSeconds { get; init; }

    /// <summary>
    /// The amount of time in minutes between now and the last
    /// successful screener progress update before the data set
    /// screening process is marked as failed.
    ///
    /// In the event that unexpected issues cause data sets to be
    /// marked as "Screening" in Admin but have failed to begin
    /// screening in the Screener API for whatever reason, this
    /// interval will allow us to compare a data set's
    /// "screener progress last checked" date against its
    /// "screener progress last updated" date, and mark it as
    /// "Failed screening" if the interval exceeds this value.
    ///
    /// In this way, we can prevent Admin from requesting screening
    /// updates forever from these broken screening attempts.
    /// </summary>
    public int ScreenerProgressUpdateFailureIntervalMinutes { get; init; }
}
