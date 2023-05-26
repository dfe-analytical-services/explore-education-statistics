#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AspectInjector.Broker;
using Aspects.Universal.Attributes;
using Aspects.Universal.Events;
using GovUk.Education.ExploreEducationStatistics.Common.Cache.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;

namespace GovUk.Education.ExploreEducationStatistics.Common.Cache
{
    /// <summary>
    /// Base caching attribute that should be extended with specific
    /// caching implementations e.g. blob storage, memory, Redis, etc.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    [Injection(typeof(CacheAspect), Inherited = true)]
    public abstract class CacheAttribute : BaseUniversalWrapperAttribute
    {
        private const BindingFlags ConstructorBindingFlags =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.CreateInstance |
            BindingFlags.OptionalParamBinding;

        /// <summary>
        /// The base type of the cache key to use.
        /// The <see cref="Key"/> must be assignable to this.
        /// </summary>
        protected virtual Type BaseKey => typeof(ICacheKey);

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

        protected override T WrapSync<T>(Func<object[], T> target, object[] args, AspectEventArgs eventArgs)
        {
            var unboxedResultType = typeof(T).GetUnboxedResultTypePath().Last();

            var cacheKey = GetCacheKey(Key, args, eventArgs.Method);

            object? cachedResult = null;
            if (!ForceUpdate)
            {
                cachedResult = Get(cacheKey, unboxedResultType);
            }

            if (cachedResult?.TryBoxToResult(typeof(T), out cachedResult) == true)
            {
                return (T)cachedResult;
            }

            try
            {
                var result = target(args);

                if (!result.TryUnboxResult(out cachedResult) || cachedResult is null)
                {
                    return result;
                }

                Set(cacheKey, cachedResult);

                return result;
            }
            catch (Exception exception)
            {
                return OnException<T>(eventArgs, exception);
            }
        }

        protected override async Task<T> WrapAsync<T>(Func<object[], Task<T>> target, object[] args, AspectEventArgs eventArgs)
        {
            var unboxedResultType = typeof(T).GetUnboxedResultTypePath().Last();

            var cacheKey = GetCacheKey(Key, args, eventArgs.Method);

            object? cachedResult = null;
            if (!ForceUpdate)
            {
                cachedResult = await GetAsync(cacheKey, unboxedResultType);
            }

            if (cachedResult?.TryBoxToResult(typeof(T), out cachedResult) == true)
            {
                return (T)cachedResult;
            }

            try
            {
                var result = await target(args);

                if (!result.TryUnboxResult(out cachedResult) || cachedResult is null)
                {
                    return result;
                }

                await SetAsync(cacheKey, cachedResult);

                return result;
            }
            catch (Exception exception)
            {
                return OnException<T>(eventArgs, exception);
            }
        }

        public abstract object? Get(ICacheKey cacheKey, Type returnType);

        public abstract void Set(ICacheKey cacheKey, object value);

        public abstract Task<object?> GetAsync(ICacheKey cacheKey, Type returnType);

        public abstract Task SetAsync(ICacheKey cacheKey, object value);

        protected virtual T OnException<T>(AspectEventArgs eventArgs, Exception exception) => throw exception;

        public ICacheKey GetCacheKey(Type cacheKeyType, object[] methodArgs, MethodBase method)
        {
            // Order constructors from most parameters to least so that we can
            // try and find the constructor with the most matching arguments first.
            var constructors = cacheKeyType.GetConstructors()
                .OrderByDescending(constructor => constructor.GetParameters().Length)
                .ToList();

            if (constructors.Count == 0)
            {
                throw new MissingMethodException(
                    $"Cache key {cacheKeyType.GetPrettyFullName()} must have at least one constructor"
                );
            }

            var parameters = GetMatchingParams(constructors, method, cacheKeyType);

            var invokeArgs = parameters
                .Select(param =>
                    {
                        if (param.Param is not null)
                        {
                            return methodArgs[param.Param.Position];
                        }

                        return param.HasDefaultValue ? Type.Missing : null;
                    }
                )
                .ToList();

            // For some reason, invoking with only missing values
            // will lead to an exception being thrown.
            if (invokeArgs.All(arg => arg == Type.Missing))
            {
                invokeArgs.Clear();
            }

            var keyInstance = Activator.CreateInstance(
                cacheKeyType,
                bindingAttr: ConstructorBindingFlags,
                args: invokeArgs.ToArray(),
                binder: null,
                culture: null
            );

            if (keyInstance is ICacheKey key)
            {
                return key;
            }

            throw new InvalidOperationException(
                $"Constructor returned null when it should instantiate {cacheKeyType.GetPrettyFullName()}"
            );
        }
        private static List<ParameterInfoScore> GetMatchingParams(
            List<ConstructorInfo> constructors,
            MethodBase method,
            Type cacheKeyType)
        {
            var methodParams = method.GetParameters();

            var cacheKeyParams = methodParams
                .Select(
                    param =>
                    {
                        var attribute = param.GetCustomAttribute<CacheKeyParamAttribute>();

                        return attribute is not null ? new CacheKeyParamInfo(param, attribute) : null;
                    }
                )
                .WhereNotNull()
                .ToList();

            List<string> matchErrors = new();

            foreach (var constructor in constructors)
            {
                var constructorParams = constructor.GetParameters();

                var constructorParamMatches = cacheKeyParams.Count > 0
                    ? MatchConstructorToCacheKeyParams(constructorParams, cacheKeyParams)
                    : MatchConstructorToMethodParams(constructorParams, methodParams);

                // Not enough arguments match the constructor's
                // parameters, so we should move onto the next one.
                if (constructorParamMatches.Count != constructorParams.Length)
                {
                    continue;
                }

                if (!constructorParamMatches.All(HasUnambiguousMatch))
                {
                    matchErrors.Add(GetMultipleMatchesError(constructorParams, constructorParamMatches));

                    continue;
                }

                // The final param candidates that we select
                // to invoke the cache key constructor with.
                var selectedParams = constructorParamMatches
                    .Select(matches => matches[0])
                    .ToList();

                var nonNullParams = selectedParams
                    .Select(param => param.Param)
                    .WhereNotNull()
                    .ToList();

                var distinctNonNullParams = nonNullParams.Distinct().ToList();

                if (nonNullParams.Count != distinctNonNullParams.Count)
                {
                    matchErrors.Add(GetCandidateParametersError(constructorParams, distinctNonNullParams));

                    continue;
                }

                return selectedParams;
            }

            if (matchErrors.Any())
            {
                throw new AmbiguousMatchException(
                    $"No constructor for cache key {cacheKeyType.GetPrettyFullName()} could be unambiguously matched. Found the following problems: \n- " +
                    matchErrors.JoinToString("\n- ")
                );
            }

            throw new MissingMemberException(
                $"No matching constructor for cache key {cacheKeyType.GetPrettyFullName()} using parameters from method: {method}"
            );
        }

        private static string GetMultipleMatchesError(
            ParameterInfo[] constructorParams,
            List<List<ParameterInfoScore>> constructorParamMatches)
        {
            var positions = constructorParamMatches
                .Select((matches, index) => !HasUnambiguousMatch(matches) ? index.ToString() : "")
                .Where(position => !position.IsNullOrEmpty())
                .JoinToString(", ");

            var constructorString = constructorParams
                .Select(param => param.ToShortString())
                .JoinToString(", ");

            return $"Constructor ({constructorString}) has multiple matches at position(s): {positions}";
        }

        private static string GetCandidateParametersError(
            IEnumerable<ParameterInfo> constructorParams,
            IEnumerable<ParameterInfo> selectedParams)
        {
            var constructorString = constructorParams
                .Select(param => param.ToShortString())
                .JoinToString(", ");

            var paramsString = selectedParams
                .Select(param => param.ToShortString())
                .JoinToString(", ");

            return
                $"Constructor ({constructorString}) has candidate parameters ({paramsString}) that could not be unambiguously matched";
        }

        private static bool HasUnambiguousMatch(List<ParameterInfoScore> matches)
        {
            return matches.Count > 1
                ? matches[0].Score > matches[1].Score
                : matches.Count == 1;
        }

        private static List<List<ParameterInfoScore>> MatchConstructorToCacheKeyParams(
            ParameterInfo[] constructorParams,
            List<CacheKeyParamInfo> cacheKeyParams)
        {
            return MatchConstructorToParams(
                constructorParams,
                constructorParam =>  cacheKeyParams
                    .Where(param => constructorParam.ParameterType.IsAssignableFrom(param.Info.ParameterType))
                    .Select(param => ScoreCacheKeyParam(param, constructorParam))
                    .OrderByDescending(param => param.Score)
                    .ToList()
            );
        }

        private static List<List<ParameterInfoScore>> MatchConstructorToMethodParams(
            ParameterInfo[] constructorParams,
            ParameterInfo[] methodParams)
        {
            return MatchConstructorToParams(
                constructorParams,
                constructorParam => methodParams
                    .Where(param => constructorParam.ParameterType.IsAssignableFrom(param.ParameterType))
                    .Select(param => ScoreParam(param, constructorParam))
                    .OrderByDescending(param => param.Score)
                    .ToList()
            );
        }

        private static List<List<ParameterInfoScore>> MatchConstructorToParams(
            ParameterInfo[] constructorParams,
            Func<ParameterInfo, List<ParameterInfoScore>> matcher)
        {
            return constructorParams.Select(
                    constructorParam =>
                    {
                        var matches = matcher(constructorParam);

                        if (!matches.Any() && (constructorParam.IsNullable() || constructorParam.HasDefaultValue))
                        {
                            matches.Add(new ParameterInfoScore(null, 0, constructorParam.HasDefaultValue));
                        }

                        return matches;
                    }
                )
                .Where(paramMatches => paramMatches.Any())
                .ToList();
        }

        private static ParameterInfoScore ScoreCacheKeyParam(
            CacheKeyParamInfo cacheKeyParam,
            ParameterInfo constructorParam)
        {
            var (cacheKeyParamInfo, cacheKeyParamAttribute) = cacheKeyParam;

            var score = 0;

            if (cacheKeyParamAttribute.Name is not null
                && cacheKeyParamAttribute.Name == constructorParam.Name)
            {
                score += 1;
            }
            else if (constructorParam.Name is not null
                     && constructorParam.Name.Equals(cacheKeyParamInfo.Name, StringComparison.OrdinalIgnoreCase))
            {
                score += 1;
            }

            return new ParameterInfoScore(cacheKeyParam.Info, score, constructorParam.HasDefaultValue);
        }

        private static ParameterInfoScore ScoreParam(
            ParameterInfo methodParam,
            ParameterInfo constructorParam)
        {
            var score = 0;

            if (constructorParam.Name is not null
                && constructorParam.Name.Equals(methodParam.Name, StringComparison.OrdinalIgnoreCase))
            {
                score += 1;
            }

            return new ParameterInfoScore(methodParam, score, constructorParam.HasDefaultValue);
        }

        private record CacheKeyParamInfo(ParameterInfo Info, CacheKeyParamAttribute Attribute);

        private record ParameterInfoScore(ParameterInfo? Param, int Score, bool HasDefaultValue = false);
    }
}
