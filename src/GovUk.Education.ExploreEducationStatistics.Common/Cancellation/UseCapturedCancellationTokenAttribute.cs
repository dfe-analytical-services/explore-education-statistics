#nullable enable
using System;
using AspectInjector.Broker;
using static GovUk.Education.ExploreEducationStatistics.Common.Cancellation.UseCapturedCancellationTokenAttribute.NoExistingTokenBehaviour;

namespace GovUk.Education.ExploreEducationStatistics.Common.Cancellation
{
    /// <summary>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    [Injection(typeof(UseCapturedCancellationTokenAspect), Priority = 20)]
    public class UseCapturedCancellationTokenAttribute : Attribute
    {
        /// <summary>
        /// </summary>
        public enum NoExistingTokenBehaviour
        {
            Throw,
            DoNothing,
        }
        
        public NoExistingTokenBehaviour NoTokenBehaviour { get; }

        public UseCapturedCancellationTokenAttribute(NoExistingTokenBehaviour noTokenBehaviour = DoNothing)
        {
            NoTokenBehaviour = noTokenBehaviour;
        }
    }
}