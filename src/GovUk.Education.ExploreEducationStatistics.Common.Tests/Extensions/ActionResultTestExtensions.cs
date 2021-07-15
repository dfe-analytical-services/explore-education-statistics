using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions
{
    public static class ActionResultTestUtils
    {
        public static T AssertOkResult<T>(this ActionResult<T> result) where T : class
        {
            Assert.IsAssignableFrom<ActionResult<T>>(result);
            Assert.IsAssignableFrom<T>(result.Value);
            return result.Value;
        }
    }
}