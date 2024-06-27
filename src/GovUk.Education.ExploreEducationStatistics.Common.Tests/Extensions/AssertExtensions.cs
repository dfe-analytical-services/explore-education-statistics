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
        public const int TimeWithinMillis = 1500;

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
            bool ignoreCollectionOrders = false,
            Expression<Func<T, object>>[]? notEqualProperties = null)
        {
            var compareLogic = new CompareLogic();
            notEqualProperties?.ForEach(compareLogic.Config.IgnoreProperty);
            compareLogic.Config.MaxDifferences = 100;
            compareLogic.Config.IgnoreCollectionOrder = ignoreCollectionOrders;
            var comparison = compareLogic.Compare(expected, actual);
            Assert.True(comparison.AreEqual, comparison.DifferencesString);
            notEqualProperties?.ForEach(notEqualField =>
            {
                var fieldGetter = notEqualField.Compile();
                var expectedValue = fieldGetter.Invoke(expected);
                var actualValue = fieldGetter.Invoke(actual);

                try
                {
                    Assert.NotEqual(expectedValue, actualValue);
                }
                catch (NotEqualException)
                {
                    throw new XunitException($"Expected values for expression {notEqualField} to not be equal, " +
                                             $"but they were both of value \"{expectedValue}\".");
                }
            });
            return true;
        }

        /// <summary>
        /// A convenience method for combining AssertDeepEqualTo with one or more inequality assertions for specific
        /// fields.  This allows us to check for a general equality rule for the majority of an object's fields, whilst
        /// also checking the opposite case for a smaller subset of fields.
        /// </summary>
        public static Expression<Func<T, object>>[] Except<T>(params Expression<Func<T, object>>[] properties)
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
            compareLogic.Config.MaxDifferences = 100;
            var comparison = compareLogic.Compare(expected, actual);
            return comparison.AreEqual;
        }

        /// <summary>
        /// Assert that the given DateTime is effectively "now", within a given tolerance of milliseconds.
        /// </summary>
        public static void AssertUtcNow(this DateTime dateTime, int withinMillis = TimeWithinMillis)
        {
            Assert.Equal(DateTime.UtcNow, dateTime, TimeSpan.FromMilliseconds(withinMillis));
        }

        /// <summary>
        /// Assert that the given DateTime is effectively "now", within a given tolerance of milliseconds.
        /// </summary>
        public static void AssertUtcNow(this DateTime? dateTime, int withinMillis = TimeWithinMillis)
        {
            Assert.NotNull(dateTime);
            dateTime.Value.AssertUtcNow(withinMillis: withinMillis);
        }

        /// <summary>
        /// Assert that the given DateTimeOffset is effectively "now", within a given tolerance of milliseconds.
        /// </summary>
        public static void AssertUtcNow(this DateTimeOffset dateTimeOffset, int withinMillis = TimeWithinMillis)
        {
            Assert.Equal(DateTimeOffset.UtcNow, dateTimeOffset, TimeSpan.FromMilliseconds(withinMillis));
        }

        /// <summary>
        /// Assert that the given DateTimeOffset is effectively "now", within a given tolerance of milliseconds.
        /// </summary>
        public static void AssertUtcNow(this DateTimeOffset? dateTimeOffset, int withinMillis = TimeWithinMillis)
        {
            Assert.NotNull(dateTimeOffset);
            dateTimeOffset.Value.AssertUtcNow(withinMillis: withinMillis);
        }
    }
}
