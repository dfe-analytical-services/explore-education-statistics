namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.Core;

public interface ICommandHandler
{
    Task<TResponse?> Handle<TCommand, TResponse>(
        Func<TCommand, CancellationToken, Task<TResponse>> commandHandler, 
        TCommand commandMessage,
        CancellationToken cancellationToken);

    Task Handle<TCommand>(
        Func<TCommand, CancellationToken, Task> commandHandler, 
        TCommand commandMessage,
        CancellationToken cancellationToken);
    
    Task<TResponse?> Handle<TResponse>(
        Func<CancellationToken, Task<TResponse>> commandHandler, 
        CancellationToken cancellationToken);
    
    Task Handle(
        Func<CancellationToken, Task> commandHandler, 
        CancellationToken cancellationToken);
}
