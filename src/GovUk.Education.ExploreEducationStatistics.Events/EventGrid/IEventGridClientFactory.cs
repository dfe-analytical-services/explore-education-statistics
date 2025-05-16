namespace GovUk.Education.ExploreEducationStatistics.Events.EventGrid;

/// <summary>
/// Creates Event Grid Publisher Clients using the specified topic configuration
/// </summary>
public interface IEventGridClientFactory
{
    IEventGridClient CreateClient(string topicEndpoint, string? topicAccessKey);
}
