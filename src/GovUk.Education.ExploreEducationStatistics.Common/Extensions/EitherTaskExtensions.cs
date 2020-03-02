using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Common.Extensions
{
    public static class EitherTaskExtensions
    {
        public static async Task<ActionResult> HandleFailures<TRight>(
            this Task<Either<ActionResult, TRight>> validationErrorsRaisingAction) where TRight : ActionResult
        {
            var result = await validationErrorsRaisingAction;
            
            return result.IsRight ? result.Right : result.Left;
        }
        
        public static async Task<ActionResult> HandleFailuresOr<T>(
            this Task<Either<ActionResult, T>> validationErrorsRaisingAction,
            Func<T, ActionResult> successFn)
        {
            var result = await validationErrorsRaisingAction;
            
            return result.IsRight ? successFn.Invoke(result.Right) : result.Left;
        }
        
        public static async Task<ActionResult<T>> HandleFailuresOrOk<T>(
            this Task<Either<ActionResult, T>> validationErrorsRaisingAction)
        {
            var result = await validationErrorsRaisingAction;
            
            return result.IsRight ? new ActionResult<T>(result.Right) : result.Left;
        }
    }
}