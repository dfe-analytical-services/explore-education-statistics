#nullable enable
using System;
using AspectInjector.Broker;
using static GovUk.Education.ExploreEducationStatistics.Common.Cancellation.AddCapturedCancellationAttribute.NoExistingTokensBehaviour;

namespace GovUk.Education.ExploreEducationStatistics.Common.Cancellation
{
    /// <summary>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    [Injection(typeof(AddCapturedCancellationAspect), Priority = 20)]
    public class AddCapturedCancellationAttribute : Attribute
    {
        /// <summary>
        /// </summary>
        public enum NoExistingTokensBehaviour
        {
            Throw,
            DoNothing,
            CreateNew,
        }
        
        public NoExistingTokensBehaviour NoExistingTokens { get; }

        public AddCapturedCancellationAttribute(NoExistingTokensBehaviour noExistingTokens = CreateNew)
        {
            NoExistingTokens = noExistingTokens;
        }
    }
}