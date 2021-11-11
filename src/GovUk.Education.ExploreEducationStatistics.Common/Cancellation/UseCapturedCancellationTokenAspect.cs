#nullable enable
using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using AspectInjector.Broker;
using static GovUk.Education.ExploreEducationStatistics.Common.Cancellation.UseCapturedCancellationTokenAttribute.NoExistingTokenBehaviour;

namespace GovUk.Education.ExploreEducationStatistics.Common.Cancellation
{
    [Aspect(Scope.Global)]
    public class UseCapturedCancellationTokenAspect
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
                                                    $"UseCapturedCancellationToken attribute must accept a " +
                                                    $"CancellationToken as its final parameter");
            }
            
            if (!CancellationAspects.Enabled)
            {
                return target(args);
            }
            
            var trigger = triggers
                .OfType<UseCapturedCancellationTokenAttribute>()
                .Single();
            
            var currentlyCapturedToken = CancellationContext.GetCurrent();

            if (currentlyCapturedToken == null && trigger.NoTokenBehaviour == Throw)
            {
                throw new ArgumentException($"Was expecting a CancellationToken to have been captured prior " +
                                            $"to method {method.Name} having been called, but was null.");
            }

            if (currentlyCapturedToken != null)
            {
                args[^1] = currentlyCapturedToken;
            }

            return target(args);
        }
    }
}