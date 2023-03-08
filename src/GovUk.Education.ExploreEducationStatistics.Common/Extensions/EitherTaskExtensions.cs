#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Common.Extensions
{
    public static class EitherTaskExtensions
    {
        public static async Task<Either<ActionResult, TRight>> OrNotFound<TRight>(this TRight? result)
            where TRight : struct
        {
            return await OrNotFound(Task.FromResult(result));
        }

        public static async Task<Either<ActionResult, TRight>> OrNotFound<TRight>(this Task<TRight?> task)
            where TRight : struct
        {
            var result = await task;

            return result is not null ? result : new NotFoundResult();
        }

        public static async Task<Either<ActionResult, TRight>> OrNotFound<TRight>(this TRight? result)
            where TRight : class?
        {
            return await OrNotFound(Task.FromResult(result));
        }

        public static async Task<Either<ActionResult, TRight>> OrNotFound<TRight>(this Task<TRight?> task)
            where TRight : class?
        {
            var result = await task;

            return result is not null ? result : new NotFoundResult();
        }

        public static async Task<ActionResult> HandleFailures<TRight>(
            this Task<Either<ActionResult, TRight>> task) where TRight : ActionResult
        {
            var result = await task;

            return result.IsRight ? result.Right : result.Left;
        }

        public static async Task<ActionResult> HandleFailuresOr<T>(
            this Task<Either<ActionResult, T>> task,
            Func<T, ActionResult> successFn)
        {
            var result = await task;

            return result.IsRight ? successFn.Invoke(result.Right) : result.Left;
        }

        public static async Task<ActionResult<T>> HandleFailuresOrOk<T>(
            this Task<Either<ActionResult, T>> task)
        {
            var result = await task;

            return result.IsRight ? new ActionResult<T>(result.Right) : result.Left;
        }

        public static async Task<ActionResult> HandleFailuresOrNoContent<T>(
            this Task<Either<ActionResult, T>> validationErrorsRaisingAction,
            bool convertNotFoundToNoContent = true)
        {
            var result = await validationErrorsRaisingAction;

            if (result.IsRight)
            {
                return new NoContentResult();
            }

            if (convertNotFoundToNoContent && result.Left is NotFoundResult)
            {
                return new NoContentResult();
            }

            return result.Left;
        }

        public static async Task<HubResult<T>> HandleFailuresOrHubResult<T>(
            this Task<Either<ActionResult, T>> task) where T : class
        {
            var result = await task;

            return result.IsRight ? new HubResult<T>(result.Right) : new HubResult<T>(result.Left);
        }
    }
}
