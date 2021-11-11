#nullable enable
using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using AspectInjector.Broker;

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
            
            if (!CancellationAspects.Enabled)
            {
                return target(args);
            }
            
            var lastParameterIsCancellationToken = 
                method.GetParameters().Length != 0
                && (method.GetParameters()[^1].ParameterType != typeof(CancellationToken)
                    || method.GetParameters()[^1].ParameterType != typeof(CancellationToken?));
            
            var trigger = triggers
                .OfType<AddTimeoutCancellationAttribute>()
                .Single();

            var timeoutToken = TimeoutToken(trigger.TimeoutMillis);
                
            if (!lastParameterIsCancellationToken)
            {
                return target(args);
            }
            
            if (args[^1] is CancellationToken currentToken)
            {
                args[^1] = CombineTimeout(currentToken, timeoutToken);
            }
            else
            {
                args[^1] = timeoutToken;
            }
            
            return target(args);
        }
        
        private static CancellationToken CombineTimeout(CancellationToken? currentToken, CancellationToken timeoutToken)
        {
            if (currentToken == null)
            {
                return timeoutToken;
            }

            return CancellationTokenSource
                .CreateLinkedTokenSource((CancellationToken) currentToken, timeoutToken)
                .Token;
        }

        private static CancellationToken TimeoutToken(int timeoutMillis)
        {
            return new CancellationTokenSource(timeoutMillis).Token;
        }
    }
}