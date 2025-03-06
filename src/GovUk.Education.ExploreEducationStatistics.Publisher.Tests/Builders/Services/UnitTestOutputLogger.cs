using System;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Tests.Builders.Services;

public class UnitTestOutputLoggerBuilder<T>
{
    public ILogger<T> Build(ITestOutputHelper output) => new XunitLogger(output);

    private class XunitLogger(ITestOutputHelper output) : ILogger<T>
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => throw new NotImplementedException();

        public bool IsEnabled(LogLevel logLevel) => throw new NotImplementedException();

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) => 
            output.WriteLine(formatter(state, exception));
    }
}
