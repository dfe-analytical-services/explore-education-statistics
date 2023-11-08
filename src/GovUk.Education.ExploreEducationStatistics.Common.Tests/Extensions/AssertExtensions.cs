#nullable enable
using System;
using System.Linq.Expressions;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using KellermanSoftware.CompareNetObjects;
using Xunit;
using Xunit.Sdk;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions
{
    public static class AssertExtensions
    {
        /**
         * Calling this method causes a Test to fail with the given message.  The equivalent of `Assert.Fail()` in
         * other testing frameworks.
         */
        public static XunitException AssertFail(string message)
        {
            throw new XunitException(message);
        }

        public static bool AssertDeepEqualTo<T>(
            this T actual,
            T expected,
            Expression<Func<T, object>>[]? ignoreProperties = null)
        {
            var compareLogic = new CompareLogic();
            ignoreProperties?.ForEach(compareLogic.Config.IgnoreProperty);
            var comparison = compareLogic.Compare(actual, expected);
            Assert.True(comparison.AreEqual, comparison.DifferencesString);
            ignoreProperties?.ForEach(ignore =>
            {
                var fieldGetter = ignore.Compile();
                var expectedIgnoredField = fieldGetter.Invoke(expected);
                var actualIgnoredField = fieldGetter.Invoke(actual);
                Assert.NotEqual(expectedIgnoredField, actualIgnoredField);
            });
            return true;
        }

        /// <summary>
        /// A convenience method for combining AssertDeepEqualTo with one or more ignorable fields, to exclude certain
        /// fields from the deep equality check.
        /// </summary>
        public static Expression<Func<T, object>>[] Ignoring<T>(params Expression<Func<T, object>>[] properties)
        {
            return properties;
        }

        public static bool IsDeepEqualTo<T>(
            this T actual,
            T expected,
            Expression<Func<T, object>>[]? ignoreProperties = null)
        {
            var compareLogic = new CompareLogic();
            ignoreProperties?.ForEach(compareLogic.Config.IgnoreProperty);
            var comparison = compareLogic.Compare(actual, expected);
            return comparison.AreEqual;
        }

        /// <summary>
        /// Assert that the given DateTime is effectively "now", within a given tolerance of milliseconds.
        /// </summary>
        public static void AssertUtcNow(this DateTime dateTime, int withinMillis = 500)
        {
            Assert.InRange(DateTime.UtcNow.Subtract(dateTime).TotalMilliseconds, 0, withinMillis);
        }

        /// <summary>
        /// Assert that the given DateTime is effectively "now", within a given tolerance of milliseconds.
        /// </summary>
        public static void AssertUtcNow(this DateTime? dateTime, int withinMillis = 500)
        {
            Assert.NotNull(dateTime);
            AssertUtcNow(dateTime!.Value, withinMillis);
        }
    }
}