#region License and Terms

// MoreLINQ - Extensions to LINQ to Objects
// Copyright (c) 2009 Atif Aziz. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;

namespace GovUk.Education.ExploreEducationStatistics.Common.Extensions
{
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Batches the source sequence into sized buckets.
        /// </summary>
        /// <typeparam name="TSource">Type of elements in <paramref name="source"/> sequence.</typeparam>
        /// <param name="source">The source sequence.</param>
        /// <param name="size">Size of buckets.</param>
        /// <returns>A sequence of equally sized buckets containing elements of the source collection.</returns>
        /// <remarks>
        /// This operator uses deferred execution and streams its results (buckets and bucket content).
        /// </remarks>
        public static IEnumerable<IEnumerable<TSource>> Batch<TSource>(this IEnumerable<TSource> source, int size)
        {
            return Batch(source, size, x => x);
        }

        /// <summary>
        /// Batches the source sequence into sized buckets and applies a projection to each bucket.
        /// </summary>
        /// <typeparam name="TSource">Type of elements in <paramref name="source"/> sequence.</typeparam>
        /// <typeparam name="TResult">Type of result returned by <paramref name="resultSelector"/>.</typeparam>
        /// <param name="source">The source sequence.</param>
        /// <param name="size">Size of buckets.</param>
        /// <param name="resultSelector">The projection to apply to each bucket.</param>
        /// <returns>A sequence of projections on equally sized buckets containing elements of the source collection.</returns>
        /// <remarks>
        /// This operator uses deferred execution and streams its results (buckets and bucket content).
        /// </remarks>
        public static IEnumerable<TResult> Batch<TSource, TResult>(this IEnumerable<TSource> source, int size,
            Func<IEnumerable<TSource>, TResult> resultSelector)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (size <= 0) throw new ArgumentOutOfRangeException(nameof(size));
            if (resultSelector == null) throw new ArgumentNullException(nameof(resultSelector));

            return _();

            IEnumerable<TResult> _()
            {
                TSource[] bucket = null;
                var count = 0;

                foreach (var item in source)
                {
                    if (bucket == null)
                    {
                        bucket = new TSource[size];
                    }

                    bucket[count++] = item;

                    // The bucket is fully buffered before it's yielded
                    if (count != size)
                    {
                        continue;
                    }

                    yield return resultSelector(bucket);

                    bucket = null;
                    count = 0;
                }

                // Return the last bucket with all remaining elements
                if (bucket != null && count > 0)
                {
                    Array.Resize(ref bucket, count);
                    yield return resultSelector(bucket);
                }
            }
        }

        public static async Task ForEachAsync<T>(this IEnumerable<T> source, Func<T, Task> func)
        {
            foreach (var item in source)
            {
                await func(item);
            }
        }

        public static async Task<IEnumerable<TResult>> SelectAsync<TSource, TResult>(
            this IEnumerable<TSource> source, Func<TSource, Task<TResult>> asyncSelector)
        {
            var result = new List<TResult>();

            foreach (var item in source)
            {
                result.Add(await asyncSelector(item));
            }

            return result;
        }
        
        /// <summary>
        /// Filter a sequence of elements asynchronously.
        /// </summary>
        /// <remarks>
        /// Do NOT use this in combination with <see cref="IQueryable{T}"/>, as Entity Framework
        /// contexts are not thread safe. The most likely outcome will be some kind of exception.
        /// Materialise the <see cref="IQueryable{T}"/> as a list or other collection first.
        /// </remarks>
        ///
        /// <param name="source">Sequence of elements to filter</param>
        /// <param name="filter">Filtering function that returns true if element should remain in sequence</param>
        /// <typeparam name="T">Type of elements in the source sequence</typeparam>
        /// <returns>Filtered sequence of elements</returns>
        public static IEnumerable<T> WhereAsync<T>(this IEnumerable<T> source, Func<T, Task<bool>> filter)
        {
            var tasks = Task.WhenAll<(T item, bool isSuccess)>(
                source.Select(async item => (item, await filter(item)))
            );

            return tasks.Result
                .Where(tuple => tuple.isSuccess)
                .Select(tuple => tuple.item);
        }

        public static IEnumerable<(T item, int index)> WithIndex<T>(this IEnumerable<T> self) =>
            self.Select((item, index) => (item, index));
        
        /// <summary>
        /// Filter a list down to distinct elements based on a property of the type.
        /// </summary>
        ///
        /// <remarks>
        /// As IEqualityComparers (as used in Linq's Distinct() method) compare with GetHashCode() rather than with
        /// Equals(), the property being used to compare distinctions against needs to produce a reliable hash code
        /// that we can use for equality.  A good property type then could be a Guid Id field, as two identical Guid Ids
        /// can then represent that 2 or more entities in the list are duplicates as they will have the same hash code.
        /// </remarks>
        /// 
        /// <param name="source">Sequence of elements to filter on a distinct property</param>
        /// <param name="propertyGetter">A supplier of a property from each entity to check for equality. The property
        /// chosen must produce the same hash code for any two elements in the source list that are considered
        /// duplicates.  A good example would be a Guid Id.</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<T> DistinctByProperty<T>(
            this IEnumerable<T> source, 
            Func<T, object> propertyGetter)
            where T : class
        {
            return source.Distinct(ComparerUtils.CreateComparerByProperty(propertyGetter));
        }
    }
}