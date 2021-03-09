using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Common.Utils
{
    /// <summary>
    /// This class adds extension methods to ILogger implementations to allow blocks of code to be easily
    /// timed and the durations logged consistently.  In addition, a current indent level is tracked and
    /// used to allow timed elements within other timed elements to be logged at an indented level
    /// relative to its parent's logging indent level, making for easier visualisation of what specific
    /// code might be time-consuming within a parent block of code.
    ///
    /// The AsyncLocal is used to track the indentation level throughout an asynchronous execution flow,
    /// meaning that the indent level is retained during the processing of async blocks of code executed by
    /// parent blocks of code.
    /// </summary>
    public static class LoggerTimingExtensions
    {
        private static readonly AsyncLocal<int> CurrentIndentLevel = new AsyncLocal<int>();
        private const int IndentLength = 4; 
            
        public static TResult TraceTime<TResult, TLogger>(this ILogger<TLogger> logger, Func<TResult> action, 
            string timingDescription, bool includeStartMessage = false)
        {
            return LogTime(logger, LogLevel.Trace, action, timingDescription, includeStartMessage);
        }
        
        public static void TraceTime<TLogger>(this ILogger<TLogger> logger, Action action, 
            string timingDescription, bool includeStartMessage = false)
        {
            LogTime(logger, LogLevel.Trace, ActionToFunc(action), timingDescription, includeStartMessage);
        }
        
        public static Task<TResult> TraceTime<TResult, TLogger>(this ILogger<TLogger> logger, Func<Task<TResult>> action, 
            string timingDescription, bool includeStartMessage = false)
        {
            return LogTime(logger, LogLevel.Trace, action, timingDescription, includeStartMessage);
        }
        
        public static Task TraceTime<TLogger>(this ILogger<TLogger> logger, Func<Task> action, 
            string timingDescription, bool includeStartMessage = false)
        {
            return LogTime(logger, LogLevel.Trace, action, timingDescription, includeStartMessage);
        }
        
        public static TResult DebugTime<TResult, TLogger>(this ILogger<TLogger> logger, Func<TResult> action, 
            string timingDescription, bool includeStartMessage = true) where TResult : class
        {
            return LogTime(logger, LogLevel.Debug, action, timingDescription, includeStartMessage);
        }
        
        public static void DebugTime<TLogger>(this ILogger<TLogger> logger, Action action, 
            string timingDescription, bool includeStartMessage = true)
        {
            LogTime(logger, LogLevel.Debug, ActionToFunc(action), timingDescription, includeStartMessage);
        }
        
        public static Task<TResult> DebugTime<TResult, TLogger>(this ILogger<TLogger> logger, Func<Task<TResult>> action, 
            string timingDescription, bool includeStartMessage = true)
        {
            return LogTime(logger, LogLevel.Debug, action, timingDescription, includeStartMessage);
        }
        
        public static Task DebugTime<TLogger>(this ILogger<TLogger> logger, Func<Task> action, 
            string timingDescription, bool includeStartMessage = true)
        { 
            return LogTime(logger, LogLevel.Debug, action, timingDescription, includeStartMessage);
        }
        
        public static TResult InfoTime<TResult, TLogger>(this ILogger<TLogger> logger, Func<TResult> action, 
            string timingDescription, bool includeStartMessage = false) where TResult : class
        {
            return LogTime(logger, LogLevel.Information, action, timingDescription, includeStartMessage);
        }
        
        public static void InfoTime<TLogger>(this ILogger<TLogger> logger, Action action, 
            string timingDescription, bool includeStartMessage = false)
        {
            LogTime(logger, LogLevel.Information, ActionToFunc(action), timingDescription, includeStartMessage);
        }
        
        public static Task<TResult> InfoTime<TResult, TLogger>(this ILogger<TLogger> logger, Func<Task<TResult>> action, 
            string timingDescription, bool includeStartMessage = false)
        {
            return LogTime(logger, LogLevel.Information, action, timingDescription, includeStartMessage);
        }
        
        public static Task InfoTime<TLogger>(this ILogger<TLogger> logger, Func<Task> action, 
            string timingDescription, bool includeStartMessage = false)
        {
            return LogTime(logger, LogLevel.Information, action, timingDescription, includeStartMessage);
        }

        private static TResult LogTime<
            TResult, TLogger>(ILogger<TLogger> logger, 
            LogLevel logLevel, 
            Func<TResult> action, 
            string timingDescription,
            bool includeStartMessage)
        {
            return LogTime(logger, logLevel, () => Task.FromResult(action.Invoke()), timingDescription, includeStartMessage).Result;
        }
        
        private static async Task LogTime<TLogger>(ILogger<TLogger> logger, 
            LogLevel logLevel, 
            Func<Task> action, 
            string timingDescription,
            bool includeStartMessage)
        {
            async Task<Unit> TaskFunc()
            {
                await action.Invoke();
                return Unit.Instance;
            }

            await LogTime(logger, logLevel, TaskFunc, timingDescription, includeStartMessage);
        }
        
        private static async Task<TResult> LogTime<
            TResult, TLogger>(ILogger<TLogger> logger,
            LogLevel logLevel, 
            Func<Task<TResult>> action, 
            string timingDescription,
            bool includeStartMessage)
        {
            if (!logger.IsEnabled(logLevel))
            {
                return await action.Invoke();
            }

            var stopwatch = Stopwatch.StartNew();
            
            var currentIndentLevel = CurrentIndentLevel.Value;
            var indentPadding = "".PadLeft(currentIndentLevel * IndentLength);
            CurrentIndentLevel.Value = currentIndentLevel + 1;

            try
            {
                if (includeStartMessage)
                {
                    logger.Log(logLevel, $"{indentPadding}Starting to time {timingDescription}");
                }

                var result = await action.Invoke();
                logger.Log(logLevel, $"{indentPadding}Took {stopwatch.Elapsed.TotalMilliseconds} milliseconds to {timingDescription}");
                return result;
            }
            finally
            {
                CurrentIndentLevel.Value = currentIndentLevel;
            }
        }

        private static Func<Unit> ActionToFunc(Action action)
        {
            return () => 
            {
                action();
                return Unit.Instance;
            };
        }
    }
}