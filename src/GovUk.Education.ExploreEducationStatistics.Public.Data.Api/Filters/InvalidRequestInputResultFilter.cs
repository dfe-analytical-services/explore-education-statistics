using System.Diagnostics.CodeAnalysis;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using JsonException = System.Text.Json.JsonException;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Filters;

/// <summary>
/// Filter to convert errors caused by invalid request input into a standard
/// validation error response. Importantly, this filter fixes the error paths to
/// point to invalid part of the request and adds error codes
/// </summary>
public class InvalidRequestInputResultFilter : IResultFilter
{
    public void OnResultExecuting(ResultExecutingContext context)
    {
        if (context.ModelState.IsValid)
        {
            return;
        }

        // A filter or endpoint has already created our standard
        // validation error response. We can bail early.
        if (context.Result is BadRequestObjectResult { Value: ValidationProblemViewModel })
        {
            return;
        }

        var errorEntries = context.ModelState
            .Where(kv => kv.Value?.ValidationState is ModelValidationState.Invalid)
            .Cast<KeyValuePair<string, ModelStateEntry>>()
            .ToList();

        var errors = new List<ErrorViewModel>();

        if (TryGetJsonError(errorEntries, out var jsonError))
        {
            errors.Add(jsonError);
        }

        if (TryGetEmptyBodyError(errorEntries, out var emptyBodyError))
        {
            errors.Add(emptyBodyError);
        }

        if (TryGetRequiredFieldError(errorEntries, context.ActionDescriptor, out var requiredFieldError))
        {
            errors.Add(requiredFieldError);
        }

        if (errors.Count == 0)
        {
            return;
        }

        var problemDetailsFactory = context.HttpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();

        var problemDetails = problemDetailsFactory.CreateValidationProblemDetails(
            context.HttpContext,
            new ModelStateDictionary()
        );

        context.Result = new BadRequestObjectResult(ValidationProblemViewModel.Create(problemDetails, errors));
    }

    public void OnResultExecuted(ResultExecutedContext context)
    {
    }

    private static bool TryGetJsonError(
        IEnumerable<KeyValuePair<string, ModelStateEntry>> errorEntries,
        [NotNullWhen(true)] out ErrorViewModel? error)
    {
        var modelError = errorEntries
            .SelectMany(entry => entry.Value.Errors)
            .FirstOrDefault(error => error.Exception is JsonException);

        if (modelError is null)
        {
            error = null;
            return false;
        }

        error = new ErrorViewModel
        {
            Code = ValidationMessages.InvalidInput.Code,
            Message = ValidationMessages.InvalidInput.Message,
            Path = GetJsonPath(modelError)
        };

        return true;
    }

    private static string GetJsonPath(ModelError error)
    {
        List<string> paths = [];

        var currentException = error.Exception;

        while (currentException is not null)
        {
            if (currentException is JsonException { Path: not null } jsonException)
            {
                paths.Add(jsonException.Path);
            }

            currentException = currentException.InnerException;
        }

        return JsonPathUtils.Concat(paths);
    }

    private static bool TryGetEmptyBodyError(
        IEnumerable<KeyValuePair<string, ModelStateEntry>> errorEntries,
        [NotNullWhen(true)] out ErrorViewModel? error)
    {
        if (!errorEntries.Any(entry => entry.Value.Errors
                .Any(e => e.ErrorMessage == ValidationMessages.NotEmptyBody.Message)))
        {
            error = null;
            return false;
        }

        error = new ErrorViewModel
        {
            Code = ValidationMessages.NotEmptyBody.Code,
            Message = ValidationMessages.NotEmptyBody.Message,
        };

        return true;
    }

    private static bool TryGetRequiredFieldError(
        IEnumerable<KeyValuePair<string, ModelStateEntry>> errorEntries,
        ActionDescriptor actionDescriptor,
        [NotNullWhen(true)] out ErrorViewModel? error)
    {
        var actionParams = actionDescriptor.Parameters
            .Where(param => param.BindingInfo?.BindingSource == BindingSource.Body
                            || param.BindingInfo?.BindingSource == BindingSource.Form)
            .Select(param => param.Name)
            .ToHashSet();

        var requiredFieldError = errorEntries
            // There are entries for the controller method's parameters which
            // need to be filtered out first. We don't want to show errors
            // for these parameters as they leak internals to API users.
            .Where(entry => !actionParams.Contains(entry.Key))
            // This isn't the ideal way of determining if we have a required field error.
            // Despite trying a few things (including using localization files), there didn't
            // seem to be an easy way to change the default error message for `RequiredAttribute`.
            // In the interest of time, this approach will have to do.
            // TODO: Work out better way to modify default data annotation error messages
            .FirstOrDefault(entry => entry.Value.Errors
                .Any(e => e.ErrorMessage.StartsWith("The") && e.ErrorMessage.EndsWith("field is required.")));

        if (requiredFieldError.Equals(default(KeyValuePair<string, ModelStateEntry>)))
        {
            error = null;
            return false;
        }

        error = new ErrorViewModel
        {
            Code = ValidationMessages.RequiredField.Code,
            Message = ValidationMessages.RequiredField.Message,
            Path = requiredFieldError.Key
        };

        return true;
    }
}
