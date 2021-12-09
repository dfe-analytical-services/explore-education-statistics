using System;
using Microsoft.AspNetCore.Mvc.Filters;
using static GovUk.Education.ExploreEducationStatistics.Common.Validators.ValidationUtils;

namespace GovUk.Education.ExploreEducationStatistics.Common.Cancellation
{
    /// <summary>
    /// A filter that converts CancellationToken-initiated cancellations into a 400 API error response. 
    /// </summary>
    public class OperationCancelledExceptionFilter : ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            if (context.Exception is OperationCanceledException)
            {
                context.ExceptionHandled = true;
                context.Result = ValidationResult("RequestCancelled");
            }
        }
    }
}