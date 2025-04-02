#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Common.Services.EventGrid;

/// <summary>
/// Creates Event Grid Publisher Clients using the specified topic configuration
/// </summary>
public interface IEventGridClientFactory
{
    IEventGridClient CreateClient(string topicEndpoint, string? topicAccessKey);
}
