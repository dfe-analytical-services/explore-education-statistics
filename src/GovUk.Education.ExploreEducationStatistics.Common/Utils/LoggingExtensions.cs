using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Common.Utils
{
    public static class LoggingExtensions
    {
        private static readonly AsyncLocal<int> CurrentIndentLevel = new AsyncLocal<int>();
        private const int IndentLength = 4; 
            
        public static TResult WithTimingTrace<TResult, TLogger>(this ILogger<TLogger> logger, Func<TResult> action, 
            string timingDescription, bool includeStartMessage = false)
        {
            return WithLogTiming(logger, LogLevel.Trace, action, timingDescription, includeStartMessage);
        }
        
        public static void WithTimingTrace<TLogger>(this ILogger<TLogger> logger, Action action, 
            string timingDescription, bool includeStartMessage = false)
        {
            WithLogTiming(logger, LogLevel.Trace, ActionToFunc(action), timingDescription, includeStartMessage);
        }
        
        public static Task<TResult> WithTimingTrace<TResult, TLogger>(this ILogger<TLogger> logger, Func<Task<TResult>> action, 
            string timingDescription, bool includeStartMessage = false)
        {
            return WithLogTiming(logger, LogLevel.Trace, action, timingDescription, includeStartMessage);
        }
        
        public static Task WithTimingTrace<TLogger>(this ILogger<TLogger> logger, Func<Task> action, 
            string timingDescription, bool includeStartMessage = false)
        {
            return WithLogTiming(logger, LogLevel.Trace, action, timingDescription, includeStartMessage);
        }
        
        public static TResult WithTimingDebug<TResult, TLogger>(this ILogger<TLogger> logger, Func<TResult> action, 
            string timingDescription, bool includeStartMessage = true) where TResult : class
        {
            return WithLogTiming(logger, LogLevel.Debug, action, timingDescription, includeStartMessage);
        }
        
        public static void WithTimingDebug<TLogger>(this ILogger<TLogger> logger, Action action, 
            string timingDescription, bool includeStartMessage = true)
        {
            WithLogTiming(logger, LogLevel.Debug, ActionToFunc(action), timingDescription, includeStartMessage);
        }
        
        public static Task<TResult> WithTimingDebug<TResult, TLogger>(this ILogger<TLogger> logger, Func<Task<TResult>> action, 
            string timingDescription, bool includeStartMessage = true)
        {
            return WithLogTiming(logger, LogLevel.Debug, action, timingDescription, includeStartMessage);
        }
        
        public static Task WithTimingDebug<TLogger>(this ILogger<TLogger> logger, Func<Task> action, 
            string timingDescription, bool includeStartMessage = true)
        { 
            return WithLogTiming(logger, LogLevel.Debug, action, timingDescription, includeStartMessage);
        }
        
        public static TResult WithTimingInformation<TResult, TLogger>(this ILogger<TLogger> logger, Func<TResult> action, 
            string timingDescription, bool includeStartMessage = false) where TResult : class
        {
            return WithLogTiming(logger, LogLevel.Information, action, timingDescription, includeStartMessage);
        }
        
        public static void WithTimingInformation<TLogger>(this ILogger<TLogger> logger, Action action, 
            string timingDescription, bool includeStartMessage = false)
        {
            WithLogTiming(logger, LogLevel.Information, ActionToFunc(action), timingDescription, includeStartMessage);
        }
        
        public static Task<TResult> WithTimingInformation<TResult, TLogger>(this ILogger<TLogger> logger, Func<Task<TResult>> action, 
            string timingDescription, bool includeStartMessage = false)
        {
            return WithLogTiming(logger, LogLevel.Information, action, timingDescription, includeStartMessage);
        }
        
        public static Task WithTimingInformation<TLogger>(this ILogger<TLogger> logger, Func<Task> action, 
            string timingDescription, bool includeStartMessage = false)
        {
            return WithLogTiming(logger, LogLevel.Information, action, timingDescription, includeStartMessage);
        }

        private static TResult WithLogTiming<
            TResult, TLogger>(ILogger<TLogger> logger, 
            LogLevel logLevel, 
            Func<TResult> action, 
            string timingDescription,
            bool includeStartMessage)
        {
            return WithLogTiming(logger, logLevel, () => Task.FromResult(action.Invoke()), timingDescription, includeStartMessage).Result;
        }
        
        private static async Task WithLogTiming<TLogger>(ILogger<TLogger> logger, 
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

            await WithLogTiming(logger, logLevel, TaskFunc, timingDescription, includeStartMessage);
        }
        
        private static async Task<TResult> WithLogTiming<
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