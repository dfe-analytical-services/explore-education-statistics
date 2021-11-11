#nullable enable
using System;
using AspectInjector.Broker;
using static GovUk.Education.ExploreEducationStatistics.Common.Cancellation.CaptureCancellationTokenAttribute.NoExistingTokenBehaviour;

namespace GovUk.Education.ExploreEducationStatistics.Common.Cancellation
{
    /// <summary>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    [Injection(typeof(CaptureCancellationTokenAspect), Priority = 10)]
    public class CaptureCancellationTokenAttribute : Attribute
    {
        /// <summary>
        /// </summary>
        public enum NoExistingTokenBehaviour
        {
            Throw,
            DoNothing,
        }
        
        public NoExistingTokenBehaviour NoTokenBehaviour { get; }

        public CaptureCancellationTokenAttribute(NoExistingTokenBehaviour noExistingTokenNoTokenBehaviour = DoNothing)
        {
            NoTokenBehaviour = noExistingTokenNoTokenBehaviour;
        }
    }
}