namespace GovUk.Education.ExploreEducationStatistics.Publisher.Configuration;

public class AppSettingsOptions
{
    public const string Section = "AppSettings";

    public required string PrivateStorageConnectionString { get; init; }

    public required string PublicStorageConnectionString { get; init; }

    public required string NotifierStorageConnectionString { get; init; }

    public required string PublisherStorageConnectionString { get; init; }

    public required string PublishReleaseContentCronSchedule { get; init; }

    public required string PublishReleasesCronSchedule { get; init; }
}
