using System;
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

        public static void AssertConflict<T>(this Either<ActionResult, T> result)
        {
            result.AssertActionResultOfType<ConflictResult, T>();
        }

        public static TRight AssertRight<TLeft, TRight>(this Either<TLeft, TRight> either, string message = null)
        {
            if (either.IsLeft)
            {
                AssertFail(message ?? $"Expected Either to be Right, but was Left with value {either.Left}");
            }

            return either.Right;
        }

        public static TRight AssertRight<TLeft, TRight>(this Either<TLeft, TRight> either, TRight expected, string message = null)
        {
            var value = either.AssertRight(message);
            Assert.Equal(expected, value);
            return value;
        }

        public static TLeft AssertLeft<TLeft, TRight>(this Either<TLeft, TRight> either, string message = null)
        {
            if (either.IsRight)
            {
                AssertFail(message ?? $"Expected Either to be Left, but was Right with value {either.Right}");
            }

            return either.Left;
        }

        public static TLeft AssertLeft<TLeft, TRight>(
            this Either<TLeft, TRight> either,
            TLeft expectedValue,
            string message = null)
        {
            var value = either.AssertLeft(message);
            Assert.Equal(expectedValue, value);
            return either.Left;
        }

        public static ActionResult AssertBadRequest<TRight>(this Either<ActionResult, TRight> either,
            params Enum[] expectedValidationErrors)
        {
            var badRequest = either.AssertActionResultOfType<BadRequestObjectResult, TRight>();
            badRequest.AssertBadRequest(expectedValidationErrors);
            return either.Left;
        }

        private static TActionResult AssertActionResultOfType<TActionResult, TRight>(this Either<ActionResult, TRight> result)
            where TActionResult : ActionResult
        {
            var actionResult = result.AssertLeft($"Expecting result to be Left when asserting result of " +
                                                 $"type {typeof(TActionResult)}");
            Assert.IsType<TActionResult>(actionResult);
            return actionResult as TActionResult;
        }
    }
}
