using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.Core;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Builders;

public class TestableCommandHandler : ICommandHandler
{
    public Task<TResponse> Handle<TCommand, TResponse>(
        Func<TCommand, CancellationToken, Task<TResponse>> commandHandler,
        TCommand commandMessage,
        CancellationToken cancellationToken
    ) => commandHandler(commandMessage, cancellationToken);

    public Task Handle<TCommand>(
        Func<TCommand, CancellationToken, Task> commandHandler,
        TCommand commandMessage,
        CancellationToken cancellationToken
    ) => commandHandler(commandMessage, cancellationToken);

    public Task<TResponse> Handle<TResponse>(
        Func<CancellationToken, Task<TResponse>> commandHandler,
        CancellationToken cancellationToken
    ) => commandHandler(cancellationToken);

    public Task Handle(
        Func<CancellationToken, Task> commandHandler,
        CancellationToken cancellationToken
    ) => commandHandler(cancellationToken);
}
