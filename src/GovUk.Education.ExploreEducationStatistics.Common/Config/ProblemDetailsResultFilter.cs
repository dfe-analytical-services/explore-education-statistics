using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;

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

        if (problemDetails is ValidationProblemViewModel validationProblemViewModel)
        {
            // If there are no OriginalDetails, this means we did not receive construct the view
            // model using a ValidationProblemDetails from the framework. Consequently, we need
            // to apply the defaults to ensure the view model is filled out properly.
            // This is mostly applicable to any use of our current ValidationUtils methods
            // which create custom bad request results with the view model already constructed.
            if (validationProblemViewModel.OriginalDetails is null)
            {
                ApplyValidationProblemDefaults(context, validationProblemViewModel);
            }

            return;
        }

        result.Value = ValidationProblemViewModel.Create(validationProblemDetails);
    }

    public void OnResultExecuted(ResultExecutedContext context)
    {
    }

    private void ApplyValidationProblemDefaults(
        ResultExecutingContext context,
        ValidationProblemViewModel validationProblem)
    {
        if (context.Controller is not ControllerBase controllerBase)
        {
            return;
        }

        var validationProblemDetails = controllerBase.ProblemDetailsFactory
                .CreateValidationProblemDetails(context.HttpContext, new ModelStateDictionary());

        validationProblem.Title ??= validationProblemDetails.Title;
        validationProblem.Type ??= validationProblemDetails.Type;
        validationProblem.Detail ??= validationProblemDetails.Detail;
        validationProblem.Instance ??= validationProblemDetails.Instance;

        if (!validationProblem.Extensions.ContainsKey("traceId") &&
            validationProblemDetails.Extensions.TryGetValue("traceId", out var traceId))
        {
            validationProblem.Extensions["traceId"] = traceId;
        }
    }
}