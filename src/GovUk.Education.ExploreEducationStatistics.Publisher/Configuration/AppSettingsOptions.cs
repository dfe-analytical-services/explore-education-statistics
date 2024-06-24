namespace GovUk.Education.ExploreEducationStatistics.Publisher.Configuration;

public class AppSettingsOptions
{
    public const string AppSettings = "AppSettings";

    public string PublishReleaseContentCronSchedule { get; init; } = null!;

    public string PublishReleasesCronSchedule { get; init; } = null!;
}
