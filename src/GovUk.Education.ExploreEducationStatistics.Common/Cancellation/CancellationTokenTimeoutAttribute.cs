#nullable enable
using System;
using System.Threading;
using AspectInjector.Broker;
using Microsoft.Extensions.Configuration;

namespace GovUk.Education.ExploreEducationStatistics.Common.Cancellation
{
    /// <summary>
    /// An attribute that, when used upon a method that accepts a
    /// <see cref="CancellationToken" /> as a parameter, provides a 
    /// CancellationToken with the given timeout in milliseconds
    /// to the method as its CancellationToken argument.
    ///
    /// If a CancellationToken has been passed to the annotated method as
    /// an argument already by the calling code, this advice will
    /// create a new linked CancellationToken with the one originally
    /// passed in, thus allowing cancellation to be initiated by either
    /// the calling code or the timeout itself. 
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    [Injection(typeof(CancellationTokenTimeoutAspect))]
    public class CancellationTokenTimeoutAttribute : Attribute
    {
        private static IConfiguration? _timeoutConfiguration;

        public int TimeoutMillis { get; }
        
        public CancellationTokenTimeoutAttribute(int timeoutMillis)
        {
            TimeoutMillis = timeoutMillis;
        }
        
        public CancellationTokenTimeoutAttribute(string configurationKey)
        {
            if (!CancellationTokenTimeoutAspect.Enabled)
            {
                return;
            }
            
            if (_timeoutConfiguration == null)
            {
                throw new ArgumentException("Timeout configuration section cannot be null when using the " +
                                            $"{nameof(CancellationTokenTimeoutAttribute)} alongside a timeout " +
                                            $"configuration key");
            }
            
            var timeoutConfig = _timeoutConfiguration.GetSection(configurationKey).Value;

            if (timeoutConfig == null)
            {
                throw new ArgumentException($"Could not find timeout configuration setting for " +
                                            $"key {configurationKey}");
            }
            
            if (!int.TryParse(timeoutConfig, out var timeoutValue))
            {
                throw new ArgumentException($"Timeout configuration setting for " +
                                            $"key {configurationKey} must be an integer");
            }
            
            TimeoutMillis = timeoutValue;
        }

        public static void SetTimeoutConfiguration(IConfiguration? timeoutConfiguration)
        {
            _timeoutConfiguration = timeoutConfiguration;
        }
    }
}