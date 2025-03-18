using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Tests.Builders.Services;

/// <summary>
/// Use this builder to construct an implementation of ILogger&lt;T&gt; that forwards all logging to the xunit test output.
/// In addition, the log statements are recorded and can be asserted.
/// </summary>
public class UnitTestOutputLoggerBuilder<T>
{
    private XunitLogger? _logger;
    
    /// <summary>
    /// Build the ILogger implementation. Pass in the instance of ITestOutputHelper injected into the xunit test class. 
    /// </summary>
    public ILogger<T> Build(ITestOutputHelper output) => _logger = new XunitLogger(output);

    private class XunitLogger(ITestOutputHelper output) : ILogger<T>
    {
        public List<LogItem> LogItems { get; } = new();
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => throw new NotImplementedException();

        public bool IsEnabled(LogLevel logLevel) => throw new NotImplementedException();

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            LogItems.Add(new LogItem(logLevel, formatter(state, exception)));
            output.WriteLine(formatter(state, exception));
        }
    }
    
    public record LogItem(LogLevel LogLevel, string Message);

    public Asserter Assert => new(_logger?.LogItems ?? throw new InvalidOperationException("Do not attempt to assert on this builder before it has been built"));
    public class Asserter(List<LogItem> logItems)
    {
        public void LoggedErrorContains(string errorMessage)
        {
            var errors = logItems.Where(l => l.LogLevel == LogLevel.Error);
            var errorsWithMessage = errors.Where(l => l.Message.Contains(errorMessage));
            Xunit.Assert.NotEmpty(errorsWithMessage);
        }
    }
}
