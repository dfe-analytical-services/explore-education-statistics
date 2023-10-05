using System;
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

        public static bool AssertDeepEqualTo<T>(this T actual, T expected)
        {
            var compareLogic = new CompareLogic();
            var comparison = compareLogic.Compare(actual, expected);
            Assert.True(comparison.AreEqual, comparison.DifferencesString);
            return true;
        }
        
        public static bool IsDeepEqualTo<T>(this T actual, T expected)
        {
            var compareLogic = new CompareLogic();
            var comparison = compareLogic.Compare(actual, expected);
            return comparison.AreEqual;
        }
        
        public static void AssertRecent(this DateTime dateTime, int withinMillis = 1500)
        {
            Assert.InRange(DateTime.UtcNow.Subtract(dateTime).Milliseconds, 0, withinMillis);
        }
        
        public static void AssertRecent(this DateTime? dateTime, int withinMillis = 1500)
        {
            Assert.True(dateTime.HasValue);
            Assert.InRange(DateTime.UtcNow.Subtract(dateTime.Value).Milliseconds, 0, withinMillis);
        }
    }
}