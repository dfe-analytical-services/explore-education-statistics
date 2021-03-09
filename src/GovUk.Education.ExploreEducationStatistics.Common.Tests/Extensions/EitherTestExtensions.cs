using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions
{
    public static class EitherTestExtensions
    {
        public static void AssertNotFound<T>(this Either<ActionResult, T> result)
        {
            Assert.True(result.IsLeft, "Expecting result to be Left when asserting NotFound");
            Assert.IsType<NotFoundResult>(result.Left);
        }
    }
}
