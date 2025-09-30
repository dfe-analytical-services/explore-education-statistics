using System.Diagnostics.CodeAnalysis;

namespace GovUk.Education.ExploreEducationStatistics.Events.EventGrid;

/// <summary>
/// Creates a client for publishing events to EventGrid using Topic settings stored in Configuration
/// </summary>
public interface IConfiguredEventGridClientFactory
{
    bool TryCreateClient(string configKey, [NotNullWhen(true)] out IEventGridClient? client);
}
