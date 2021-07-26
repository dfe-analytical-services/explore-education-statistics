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
            result.AssertActionResultOfType<NotFoundResult, T>();
        }
        
        public static void AssertForbidden<T>(this Either<ActionResult, T> result)
        {
            result.AssertActionResultOfType<ForbidResult, T>();
        }
        
        public static TRight AssertRight<TLeft, TRight>(this Either<TLeft, TRight> either, string message = null)
        {
            if (either.IsLeft)
            {
                AssertFail(message ?? $"Expected Either to be Right, but was Left with value {either.Left}");
            }
            
            return either.Right;
        }
        
        public static TLeft AssertLeft<TLeft, TRight>(this Either<TLeft, TRight> either, string message = null)
        {
            if (either.IsRight)
            {
                AssertFail(message ?? $"Expected Either to be Left, but was Right with value {either.Right}");
            }
            
            return either.Left;
        }
        
        private static void AssertActionResultOfType<TActionResult, TRight>(this Either<ActionResult, TRight> result)
            where TActionResult : ActionResult
        {
            var actionResult = result.AssertLeft($"Expecting result to be Left when asserting result of " +
                                                 $"type {typeof(TActionResult)}");
            Assert.IsType<TActionResult>(actionResult);
        }
    }
}
