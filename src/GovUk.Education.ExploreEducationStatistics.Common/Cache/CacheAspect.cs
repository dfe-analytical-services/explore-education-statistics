#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AspectInjector.Broker;
using GovUk.Education.ExploreEducationStatistics.Common.Cache.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;

namespace GovUk.Education.ExploreEducationStatistics.Common.Cache
{
    [Aspect(Scope.Global)]
    public class CacheAspect
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
            [Argument(Source.Target)] Func<object[], object> target,
            [Argument(Source.Arguments)] object[] args,
            [Argument(Source.Metadata)] MethodBase method,
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

            var cacheTriggers = triggers
                .OfType<CacheAttribute>()
                .Distinct()
                .OrderByDescending(c => c.Priority)
                .ToList();

            // Keep track of the the cache keys so that we
            // don't need to recalculate them later on
            // (this could be expensive due to reflection).
            var cacheKeys = new Dictionary<Type, ICacheKey>();

            object? cachedResult = null;

            // May be using boxed types, and these may be nested, so we
            // recursively go through all the types to get the real
            // return type that we're interested in caching.
            var cachedReturnType = returnType.GetUnboxedResultTypePath().Last();

            foreach (var cacheTrigger in cacheTriggers)
            {
                // Don't attempt to get a cached value from any triggers that are requesting that the cached value be
                // updated rather than fetched from the cache. 
                if (cacheTrigger.ForceUpdate)
                {
                    continue;
                }
                
                var cacheKey = cacheKeys.GetOrSet(
                    cacheTrigger.Key,
                    () => GetCacheKey(cacheTrigger.Key, args, method)
                );

                var triggerResult = cacheTrigger.Get(cacheKey, cachedReturnType).Result;

                if (triggerResult is null)
                {
                    continue;
                }

                cachedResult = triggerResult;
                break;
            }

            if (cachedResult?.TryBoxToResult(returnType, out cachedResult) == true)
            {
                return cachedResult;
            }

            // If no cached result could be found, then we run the real
            // method and get its result so that we can cache it.
            var result = target(args);

            if (!result.TryUnboxResult(out cachedResult) || cachedResult is null)
            {
                return result;
            }

            Task.WaitAll(
                cacheTriggers.Select(
                        cacheTrigger =>
                        {
                            var cacheKey = cacheKeys.GetOrSet(
                                cacheTrigger.Key,
                                () => GetCacheKey(cacheTrigger.Key, args, method)
                            );

                            return cacheTrigger.Set(cacheKey, cachedResult);
                        }
                    )
                    .ToArray()
            );

            return result;
        }

        private ICacheKey GetCacheKey(Type cacheKeyType, object[] methodArgs, MethodBase method)
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
