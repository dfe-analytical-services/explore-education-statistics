#nullable enable
using System;
using AspectInjector.Broker;
using Microsoft.Extensions.Configuration;

namespace GovUk.Education.ExploreEducationStatistics.Common.Cancellation
{
    /// <summary>
    /// An attribute that, when used upon a method that accepts a
    /// CancellationToken as its final parameter, provides a 
    /// CancellationToken with the given timeout in milliseconds
    /// to the method as its CancellationToken argument.
    ///
    /// If a CancellationToken has been passed to this method as an
    /// argument already by the calling code, this attribute will
    /// create a new linked CancellationToken with the one originally
    /// passed in, thus allowing cancellation to be initiated by either
    /// the calling code or the timeout itself. 
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    [Injection(typeof(AddTimeoutCancellationAspect))]
    public class AddTimeoutCancellationAttribute : Attribute
    {
        private static IConfiguration? _timeoutConfiguration;

        public int TimeoutMillis { get; }
        
        public AddTimeoutCancellationAttribute(int timeoutMillis)
        {
            TimeoutMillis = timeoutMillis;
        }
        
        public AddTimeoutCancellationAttribute(string configurationKey)
        {
            if (!AddTimeoutCancellationAspect.Enabled)
            {
                return;
            }
            
            if (_timeoutConfiguration == null)
            {
                throw new ArgumentException("TimeoutConfiguration section cannot be null when using the " +
                                            "AddTimeoutCancellation attribute alongside a timeout configuration key");
            }
            
            var timeoutConfig = _timeoutConfiguration.GetSection(configurationKey).Value;

            if (timeoutConfig == null)
            {
                throw new ArgumentException($"Could not find TimeoutConfiguration configuration setting for " +
                                            $"key {configurationKey}");
            }
            
            if (!int.TryParse(timeoutConfig, out var timeoutValue))
            {
                throw new ArgumentException($"TimeoutConfiguration configuration setting for " +
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