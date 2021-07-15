using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions.AssertExtensions;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions
{
    public static class EitherTestExtensions
    {
        public static void AssertNotFound<T>(this Either<ActionResult, T> result)
        {
            Assert.True(result.IsLeft, "Expecting result to be Left when asserting NotFound");
            Assert.IsType<NotFoundResult>(result.Left);
        }
        
        public static TRight AssertRight<TLeft, TRight>(this Either<TLeft, TRight> either)
        {
            if (either.IsLeft)
            {
                AssertFail($"Expected Either to be Right, but was Left with value {either.Left}");
            }
            
            return either.Right;
        }
    }
}
