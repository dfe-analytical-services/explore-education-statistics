using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.Core;

public class CommandHandler(ILogger<CommandHandler> logger) : ICommandHandler
{
    public async Task<TResponse> Handle<TCommand, TResponse>(
        Func<TCommand, CancellationToken, Task<TResponse>> commandHandler, 
        TCommand commandMessage,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Handling command: {@Command}", commandMessage);
        try
        {
            var response = await commandHandler(commandMessage, cancellationToken);
            logger.LogDebug("Handled command: {@Command}  Response:{@Response}", commandMessage, response);
            return response;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error handling command: {@Command}", commandMessage);
            throw;
        }
    }
    
    public async Task Handle<TCommand>(
        Func<TCommand, CancellationToken, Task> commandHandler, 
        TCommand commandMessage,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Handling command: {@Command}", commandMessage);
        try
        {
            await commandHandler(commandMessage, cancellationToken);
            logger.LogDebug("Handled command: {@Command}", commandMessage);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error handling command: {@Command}", commandMessage);
            throw;
        }
    }

    public async Task<TResponse> Handle<TResponse>(
        Func<CancellationToken, Task<TResponse>> commandHandler,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Handling command.");
        try
        {
            var response = await commandHandler(cancellationToken);
            logger.LogDebug("Handled command: Response:{@Response}", response);
            return response;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error handling command.");
            throw;
        }
    }
    public async Task Handle(
        Func<CancellationToken, Task> commandHandler,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Handling command.");
        try
        {
            await commandHandler(cancellationToken);
            logger.LogDebug("Handled command.");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error handling command.");
            throw;
        }
    }
}
