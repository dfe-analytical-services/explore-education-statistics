#nullable enable
using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using AspectInjector.Broker;
using static GovUk.Education.ExploreEducationStatistics.Common.Cancellation.CancellationAspects;

namespace GovUk.Education.ExploreEducationStatistics.Common.Cancellation
{
    [Aspect(Scope.Global)]
    public class AddTimeoutCancellationAspect
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
                throw new ArgumentException($"Method {method.Name} annotated with the " +
                                            $"AddTimeoutCancellation attribute must accept a CancellationToken as " +
                                            $"its final parameter");
            }
            
            if (!Enabled)
            {
                return target(args);
            }
            
            var trigger = triggers
                .OfType<AddTimeoutCancellationAttribute>()
                .Single();

            var timeoutToken = TimeoutToken(trigger.TimeoutMillis);
            var passedInToken = args[^1] as CancellationToken?;
            var newArgs = args.ToArray();
            newArgs[^1] = CombineTokens(passedInToken, timeoutToken);
            return target(newArgs);
        }

        private static CancellationToken TimeoutToken(int timeoutMillis)
        {
            return new CancellationTokenSource(timeoutMillis).Token;
        }
    }
}