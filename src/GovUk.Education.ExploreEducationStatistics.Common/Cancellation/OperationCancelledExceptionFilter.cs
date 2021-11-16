using System;
using GovUk.Education.ExploreEducationStatistics.Common.Validators;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GovUk.Education.ExploreEducationStatistics.Common.Cancellation
{
    public class OperationCancelledExceptionFilter : ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            if (context.Exception is OperationCanceledException)
            {
                context.ExceptionHandled = true;
                context.Result = ValidationUtils.ValidationResult("RequestCancelled");
            }
        }
    }
}