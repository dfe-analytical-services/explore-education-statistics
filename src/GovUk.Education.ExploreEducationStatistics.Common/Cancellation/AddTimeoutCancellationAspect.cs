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
        /// <summary>
        /// Enables the AddTimeoutCancellation aspect.
        /// <para>
        /// This is set to false by default so that test code
        /// isn't affected by this aspect. It should be set to
        /// true in your application startup, or if your tests
        /// are concerned with testing cancellations.
        /// </para>
        /// </summary>
        public static bool Enabled { get; set; }
        
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
        
        private static CancellationToken CombineTokens(params CancellationToken?[] tokens)
        {
            var nonNullTokens = tokens
                .Where(token => token != null)
                .Cast<CancellationToken>()
                .ToArray();

            if (nonNullTokens.Length == 0)
            {
                return new CancellationToken();
            }

            if (nonNullTokens.Length == 1)
            {
                return nonNullTokens[0];
            }

            return CancellationTokenSource
                .CreateLinkedTokenSource(nonNullTokens)
                .Token;
        }
    }
}