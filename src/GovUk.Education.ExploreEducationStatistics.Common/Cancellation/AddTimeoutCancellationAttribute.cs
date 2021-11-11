#nullable enable
using System;
using AspectInjector.Broker;

namespace GovUk.Education.ExploreEducationStatistics.Common.Cancellation
{
    /// <summary>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    [Injection(typeof(AddTimeoutCancellationAspect), Priority = 20)]
    public class AddTimeoutCancellationAttribute : Attribute
    {
        /// <summary>
        /// </summary>
        public int TimeoutMillis { get; }
        
        public AddTimeoutCancellationAttribute(int timeoutMillis)
        {
            TimeoutMillis = timeoutMillis;
        }
    }
}