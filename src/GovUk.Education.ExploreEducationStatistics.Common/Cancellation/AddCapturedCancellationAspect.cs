#nullable enable
using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using AspectInjector.Broker;
using static GovUk.Education.ExploreEducationStatistics.Common.Cancellation.CancellationAspects;
using static GovUk.Education.ExploreEducationStatistics.Common.Cancellation.AddCapturedCancellationAttribute.NoExistingTokensBehaviour;

namespace GovUk.Education.ExploreEducationStatistics.Common.Cancellation
{
    [Aspect(Scope.Global)]
    public class AddCapturedCancellationAspect
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
                                                    $"AddCapturedCancellation attribute must accept a " +
                                                    $"CancellationToken as its final parameter");
            }
            
            if (!Enabled)
            {
                return target(args);
            }
            
            var trigger = triggers
                .OfType<AddCapturedCancellationAttribute>()
                .Single();
            
            var currentlyCapturedToken = CancellationContext.GetCurrent();
            var passedInToken = args[^1] as CancellationToken?;
            
            if (currentlyCapturedToken == null && passedInToken == null)
            {
                if (Throw == trigger.NoExistingTokens)
                {
                    throw new ArgumentException($"Was expecting a CancellationToken to have been captured prior " +
                                                $"to method {method.Name} having been called, but was null.");
                }

                if (DoNothing == trigger.NoExistingTokens)
                {
                    return target(args);
                }
            }

            var newArgs = args.ToArray();
            newArgs[^1] = CombineTokens(passedInToken, currentlyCapturedToken);
            return target(newArgs);
        }
    }
}