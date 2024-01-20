using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GovUk.Education.ExploreEducationStatistics.Common.Config;

/// <summary>
/// A filter that handles conversion of any problem details to our customised view models.
/// </summary>
public class ProblemDetailsResultFilter : IResultFilter
{
    public void OnResultExecuting(ResultExecutingContext context)
    {
        if (context.Result is not ObjectResult result)
        {
            return;
        }

        if (result.Value is not ProblemDetails problemDetails)
        {
            return;
        }

        // If for some reason, there is a mismatch with the
        // HTTP status code, make sure these are aligned.
        problemDetails.Status = result.StatusCode;

        if (problemDetails is not ValidationProblemDetails validationProblemDetails)
        {
            return;
        }

        // Result is already the right view model type, don't need to do anything else.
        if (problemDetails is ValidationProblemViewModel)
        {
            return;
        }

        result.Value = ValidationProblemViewModel.Create(validationProblemDetails);
    }

    public void OnResultExecuted(ResultExecutedContext context)
    {
    }
}
