#nullable enable
using System;
using AspectInjector.Broker;
using static GovUk.Education.ExploreEducationStatistics.Common.Cancellation.AddCapturedCancellationAttribute.NoCapturedTokenBehaviour;

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
        public enum NoCapturedTokenBehaviour
        {
            Throw,
            DoNothing,
            CreateNew,
        }
        
        public NoCapturedTokenBehaviour NoCapturedBehaviour { get; }

        public AddCapturedCancellationAttribute(NoCapturedTokenBehaviour noCapturedBehaviour = CreateNew)
        {
            NoCapturedBehaviour = noCapturedBehaviour;
        }
    }
}