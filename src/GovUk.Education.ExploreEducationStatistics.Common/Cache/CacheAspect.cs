#nullable enable
using System;
using System.Reflection;
using System.Threading.Tasks;
using AspectInjector.Broker;
using Aspects.Universal.Aspects;

namespace GovUk.Education.ExploreEducationStatistics.Common.Cache
{
    [Aspect(Scope.Global)]
    public class CacheAspect : BaseUniversalWrapperAspect
    {
        private const BindingFlags ConstructorBindingFlags =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.CreateInstance |
            BindingFlags.OptionalParamBinding;

        /// <summary>
        /// Enables cache attribute processing.
        /// <para>
        /// This is set to false by default so that test code
        /// isn't affected by this aspect. It should be set to
        /// true in your application startup, or if your tests
        /// are concerned with testing caching details.
        /// </para>
        /// </summary>
        public static bool Enabled { get; set; }

        [Advice(Kind.Around)]
        public object Handle(
            [Argument(Source.Instance)] object instance,
            [Argument(Source.Type)] Type type,
            [Argument(Source.Metadata)] MethodBase method,
            [Argument(Source.Target)] Func<object[], object> target,
            [Argument(Source.Name)] string name,
            [Argument(Source.Arguments)] object[] args,
            [Argument(Source.ReturnType)] Type returnType,
            [Argument(Source.Triggers)] Attribute[] triggers)
        {
            if (!Enabled)
            {
                return target(args);
            }

            if (typeof(void) == returnType)
            {
                throw new ArgumentException("Method return type cannot be void");
            }

            if (typeof(Task).IsAssignableFrom(returnType) && !returnType.IsConstructedGenericType)
            {
                throw new ArgumentException("Method return type cannot be Task. Consider using Task<TResult>");
            }

            return BaseHandle(instance, type, method, target, name, args, returnType, triggers);
        }
    }
}
