#nullable enable
using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using AspectInjector.Broker;
using Microsoft.EntityFrameworkCore.Internal;

namespace GovUk.Education.ExploreEducationStatistics.Common.Cancellation
{
    [Aspect(Scope.Global)]
    public class CancellationTokenTimeoutAspect
    {
        /// <summary>
        /// Enables the <see cref="CancellationTokenTimeoutAspect" /> Aspect that supports use of the
        /// <see cref="CancellationTokenTimeoutAttribute" /> to annotate methods.
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
            [Argument(Source.Triggers)] Attribute[] triggers)
        {
            var cancellationTokenParameters = method
                .GetParameters()
                .Where(parameter => parameter.ParameterType == typeof(CancellationToken)
                                    || parameter.ParameterType == typeof(CancellationToken?))
                .ToArray();
            
            if (cancellationTokenParameters.Count() != 1)
            {
                throw new ArgumentException($"Method {method.Name} annotated with the " +
                                            $"{nameof(CancellationTokenTimeoutAspect)} attribute must " +
                                            $"accept a single {nameof(CancellationToken)} parameter");
            }

            var cancellationTokenParamPosition = method
                .GetParameters()
                .ToList()
                .IndexOf(cancellationTokenParameters.Single());
            
            if (!Enabled)
            {
                return target(args);
            }
            
            var trigger = triggers
                .OfType<CancellationTokenTimeoutAttribute>()
                .Single();

            var timeoutToken = new CancellationTokenSource(trigger.TimeoutMillis).Token;
            var passedInToken = args[cancellationTokenParamPosition] as CancellationToken?;
            var newArgs = args.ToArray();
            newArgs[cancellationTokenParamPosition] = CombineTokens(passedInToken, timeoutToken);
            return target(newArgs);
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