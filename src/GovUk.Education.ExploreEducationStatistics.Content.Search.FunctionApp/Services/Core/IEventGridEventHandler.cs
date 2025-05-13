using Azure.Messaging.EventGrid;
using Microsoft.Azure.Functions.Worker;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.Core;

public interface IEventGridEventHandler
{
    Task<TResponse> Handle<TPayload, TResponse>(
        FunctionContext context,
        EventGridEvent eventGridEvent,
        Func<TPayload, CancellationToken, Task<TResponse>> handler);
}
