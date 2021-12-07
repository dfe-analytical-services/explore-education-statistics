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
    }
}