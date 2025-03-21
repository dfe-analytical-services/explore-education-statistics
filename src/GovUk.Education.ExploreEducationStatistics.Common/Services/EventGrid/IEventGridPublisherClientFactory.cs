namespace GovUk.Education.ExploreEducationStatistics.Common.Services.EventGrid;

public interface IEventGridPublisherClientFactory
{
    IEventGridPublisherClient CreateClient(string topicEndpoint, string? topicAccessKey);
}
