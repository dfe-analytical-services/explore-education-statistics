namespace GovUk.Education.ExploreEducationStatistics.Publisher.Options;

public class AppOptions
{
    public const string Section = "App";

    public required string PrivateStorageConnectionString { get; init; }

    public required string PublicStorageConnectionString { get; init; }

    public required string NotifierStorageConnectionString { get; init; }

    public required string PublisherStorageConnectionString { get; init; }

    public required string PublishScheduledReleasesFunctionCronSchedule { get; init; }

    public required string StageScheduledReleasesFunctionCronSchedule { get; init; }
}
