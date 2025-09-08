using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Builders;

/// <summary>
/// Use this builder to construct an implementation of <summary cref="ILogger{T}" /> where
/// the log statements are recorded and can be asserted.
/// </summary>
public class LoggerMockBuilder<T>
{
    private MockLogger? _logger;
    private Action<string>? _logAction;

    /// <summary>
    /// Build the <summary cref="ILogger{T}" /> instance. Pass in the instance of <summary cref="ITestOutputHelper" /> injected into the xunit test class.
    /// </summary>
    public ILogger<T> Build() => _logger = new MockLogger(_logAction);

    public LoggerMockBuilder<T> WithLogAction(Action<string>? logAction)
    {
        _logAction = logAction;
        return this;
    }

    private class MockLogger(Action<string>? output = null) : ILogger<T>
    {
        public List<LogItem> LogItems { get; } = new();

        public IDisposable BeginScope<TState>(TState state)
            where TState : notnull => throw new NotImplementedException();

        public bool IsEnabled(LogLevel logLevel) => throw new NotImplementedException();

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter
        )
        {
            LogItems.Add(new LogItem(logLevel, formatter(state, exception)));
            output?.Invoke(formatter(state, exception));
        }
    }

    public record LogItem(LogLevel LogLevel, string Message);

    public Asserter Assert =>
        new(
            _logger?.LogItems
                ?? throw new InvalidOperationException(
                    "Do not attempt to assert on this builder before it has been built"
                )
        );

    public class Asserter(List<LogItem> logItems)
    {
        public void LoggedErrorContains(string errorMessage)
        {
            var errors = logItems.Where(l => l.LogLevel == LogLevel.Error);
            var errorsWithMessage = errors.Where(l => l.Message.Contains(errorMessage));
            Xunit.Assert.NotEmpty(errorsWithMessage);
        }

        public void LoggedInfoContains(string message)
        {
            var infos = logItems.Where(l => l.LogLevel == LogLevel.Information);
            var infosWithMessage = infos.Where(l => l.Message.Contains(message));
            Xunit.Assert.NotEmpty(infosWithMessage);
        }

        public void LoggedDebugContains(string message)
        {
            var infos = logItems.Where(l => l.LogLevel == LogLevel.Debug);
            var infosWithMessage = infos.Where(l => l.Message.Contains(message));
            Xunit.Assert.NotEmpty(infosWithMessage);
        }
    }
}
