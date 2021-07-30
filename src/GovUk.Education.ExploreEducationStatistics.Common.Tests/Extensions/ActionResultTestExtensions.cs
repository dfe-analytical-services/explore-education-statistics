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
        
        public static void AssertNotFoundResult<T>(this ActionResult<T> result) where T : class
        {
            Assert.IsAssignableFrom<NotFoundResult>(result.Result);
        }
        
        public static void AssertNoContentResult(this ActionResult result)
        {
            Assert.IsAssignableFrom<NoContentResult>(result);
        }
    }
}