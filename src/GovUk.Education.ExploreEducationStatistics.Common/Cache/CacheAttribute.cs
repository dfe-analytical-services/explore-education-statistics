#nullable enable
using System;
using System.Threading.Tasks;
using AspectInjector.Broker;
using GovUk.Education.ExploreEducationStatistics.Common.Cache.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;

namespace GovUk.Education.ExploreEducationStatistics.Common.Cache
{
    /// <summary>
    /// Base caching attribute that should be extended with specific
    /// caching implementations e.g. blob storage, memory, Redis, etc.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    [Injection(typeof(CacheAspect), Inherited = true)]
    public abstract class CacheAttribute : Attribute
    {
        /// <summary>
        /// The base type of the cache key to use.
        /// The <see cref="Key"/> must be assignable to this.
        /// </summary>
        protected virtual Type BaseKey => typeof(ICacheKey);

        /// <summary>
        /// Cache with highest priority is checked first.
        /// 0 - Lowest, 255 - Highest.
        /// </summary>
        public byte Priority { get; init; } = 0;

        /// <summary>
        /// The explicit type of the caching key to use. It must be assignable
        /// to the <see cref="BaseKey"/>.
        /// <para>
        /// We construct the caching key by passing in the method parameters into
        /// one of its constructors. We will try our best to match each of
        /// these based on the their types and names.
        /// </para>
        ///<para>
        /// In situations where it is ambiguous which parameters to use, we will
        /// throw an appropriate exception. For more control (and to avoid ambiguity),
        /// use <see cref="CacheKeyParamAttribute"/> to configure which parameters
        /// should be passed through. We recommend that you keep your constructor
        /// as simple as possible as the matching algorithm is not perfect and
        /// will probably need updates for additional use-cases in the future.
        /// </para>
        /// </summary>
        public Type Key { get; }
        
        /// <summary>
        /// Flag signifying that this caching attribute forces a cache update.
        /// When an attribute with this flag set is invoked, it will retrieve a fresh
        /// value rather than a cached item and then set or update it in the cache,
        /// therefore always caching and then supplying a freshly retrieved value.
        /// </summary>
        public bool ForceUpdate { get; }

        protected CacheAttribute(Type key, bool forceUpdate)
        {
            Key = key;
            ForceUpdate = forceUpdate;
            ValidateOptions();
        }

        private void ValidateOptions()
        {
            if (Key is null)
            {
                throw new ArgumentException("Cache key type cannot be null");
            }

            if (!BaseKey.IsAssignableFrom(Key))
            {
                throw new ArgumentException($"Cache key type {Key.GetPrettyFullName()} must be assignable to {BaseKey.GetPrettyFullName()}");
            }
        }

        public abstract Task<object?> Get(ICacheKey cacheKey, Type returnType);

        public abstract Task Set(ICacheKey cacheKey, object value);
    }
}
