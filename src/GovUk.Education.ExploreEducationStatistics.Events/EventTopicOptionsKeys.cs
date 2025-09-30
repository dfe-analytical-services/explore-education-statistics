using GovUk.Education.ExploreEducationStatistics.Events.EventGrid;

namespace GovUk.Education.ExploreEducationStatistics.Events;

/// <summary>
/// Defines constants that represent keys for different <see cref="EventTopicOptions"/>.
/// These keys are used by <see cref="IEvent"/> implementations to resolve the topic endpoint from configuration sources,
/// including environment variables and the `appsettings.json` file.
/// </summary>
public static class EventTopicOptionsKeys
{
    public const string PublicationChanged = "PublicationChangedEvent";
    public const string ReleaseChanged = "ReleaseChangedEvent";
    public const string ReleaseVersionChanged = "ReleaseVersionChangedEvent";
    public const string ThemeChanged = "ThemeChangedEvent";
}
