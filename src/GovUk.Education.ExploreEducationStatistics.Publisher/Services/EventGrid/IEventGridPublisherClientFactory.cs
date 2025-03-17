namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services.EventGrid;

public interface IEventGridPublisherClientFactory
{
    IEventGridPublisherClient CreateClient(string topicEndpoint, string? topicAccessKey);
}
