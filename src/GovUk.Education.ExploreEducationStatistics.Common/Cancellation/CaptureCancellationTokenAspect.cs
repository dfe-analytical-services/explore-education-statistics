#nullable enable
using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using AspectInjector.Broker;
using static GovUk.Education.ExploreEducationStatistics.Common.Cancellation.CaptureCancellationTokenAttribute.NoExistingTokenBehaviour;

namespace GovUk.Education.ExploreEducationStatistics.Common.Cancellation
{
    [Aspect(Scope.Global)]
    public class CaptureCancellationTokenAspect
    {
        [Advice(Kind.Around)]
        public object Handle(
            [Argument(Source.Target)] Func<object[], object> target,
            [Argument(Source.Arguments)] object[] args,
            [Argument(Source.Metadata)] MethodBase method,
            [Argument(Source.ReturnType)] Type returnType,
            [Argument(Source.Triggers)] Attribute[] triggers)
        {
            if (method.GetParameters().Length == 0 
                || (method.GetParameters()[^1].ParameterType != typeof(CancellationToken)
                    && method.GetParameters()[^1].ParameterType != typeof(CancellationToken?)))
            {
                throw new ArgumentException($"Method {method.Name}  annotated with the " +
                                                    $"CaptureCancellationToken attribute must accept a " +
                                                    $"CancellationToken as its final parameter");
            }
            
            if (!CancellationAspects.Enabled)
            {
                return target(args);
            }
            
            var trigger = triggers
                .OfType<CaptureCancellationTokenAttribute>()
                .Single();
            
            var currentlyCapturedToken = CancellationContext.GetCurrent();

            var tokenToCapture = args[^1] as CancellationToken?;

            if (tokenToCapture == null && trigger.NoTokenBehaviour == Throw)
            {
                throw new ArgumentException($"Was expecting a CancellationToken to be supplied to method " +
                                            $"{method.Name} but was null.");
            }
            
            CancellationContext.SetCurrent(tokenToCapture);

            try
            {
                return target(args);
            }
            finally
            {
                CancellationContext.SetCurrent(currentlyCapturedToken);
            }
        }
    }
}