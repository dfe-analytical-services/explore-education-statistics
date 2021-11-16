#nullable enable
using System;
using AspectInjector.Broker;
using Microsoft.Extensions.Configuration;

namespace GovUk.Education.ExploreEducationStatistics.Common.Cancellation
{
    /// <summary>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    [Injection(typeof(AddTimeoutCancellationAspect), Priority = 20)]
    public class AddTimeoutCancellationAttribute : Attribute
    {
        private static IConfiguration? _timeoutConfiguration;

        /// <summary>
        /// Defaults to 1 minute.
        /// </summary>
        public int TimeoutMillis { get; } = 60000;
        
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
            
            TimeoutMillis = int.Parse(timeoutConfig);
        }

        public static void SetTimeoutConfiguration(IConfiguration timeoutConfiguration)
        {
            _timeoutConfiguration = timeoutConfiguration;
        }
    }
}