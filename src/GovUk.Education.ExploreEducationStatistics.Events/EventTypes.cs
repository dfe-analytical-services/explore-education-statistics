namespace GovUk.Education.ExploreEducationStatistics.Events;

/// <summary>
/// Defines constants for event types that are published to the Publication Changed event topic.
/// </summary>
public static class PublicationChangedEventTypes
{
    public const string PublicationArchived = "publication-archived";
    public const string PublicationChanged = "publication-changed";
    public const string PublicationDeleted = "publication-deleted";
    public const string PublicationLatestPublishedReleaseReordered = "publication-latest-published-release-reordered";
    public const string PublicationRestored = "publication-restored";
}

/// <summary>
/// Defines constants for event types that are published to the Release Changed event topic.
/// </summary>
public static class ReleaseChangedEventTypes
{
    public const string ReleaseSlugChanged = "release-slug-changed";
}

/// <summary>
/// Defines constants for event types that are published to the Release Version Changed event topic.
/// </summary>
public static class ReleaseVersionChangedEventTypes
{
    public const string ReleaseVersionPublished = "release-version-published";
}

/// <summary>
/// Defines constants for event types that are published to the Theme Changed event topic.
/// </summary>
public static class ThemeChangedEventTypes
{
    public const string ThemeChanged = "theme-changed";
}
