using System.Diagnostics.CodeAnalysis;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using Microsoft.AspNetCore.Mvc;
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
public class InvalidRequestInputFilter : IAsyncActionFilter
{
    public Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // A filter or endpoint has already created our standard
        // validation error response. We can bail early.
        if (context.Result is BadRequestObjectResult { Value: ValidationProblemViewModel })
        {
            return next();
        }

        var errors = new List<ErrorViewModel>();

        errors.AddRange(GetUnknownQueryParameterErrors(context));

        if (context.ModelState.IsValid && errors.Count == 0)
        {
            return next();
        }

        var invalidModelState = context
            .ModelState.Where(kv => kv.Value?.ValidationState is ModelValidationState.Invalid)
            .Cast<KeyValuePair<string, ModelStateEntry>>()
            .ToDictionary();

        if (TryGetJsonError(invalidModelState, out var jsonError))
        {
            errors.Add(jsonError);
        }

        if (TryGetEmptyBodyError(invalidModelState, out var emptyBodyError))
        {
            errors.Add(emptyBodyError);
        }

        // There can be error entries for the controller method's parameters
        // which need to be filtered out first (which otherwise leak internals).
        var invalidErrorKeys = context
            .ActionDescriptor.Parameters.Where(param => param.ParameterType.IsComplex())
            .Select(param => param.Name)
            .ToHashSet();

        invalidModelState = invalidModelState
            .Where(entry => !invalidErrorKeys.Contains(entry.Key))
            .ToDictionary(kv => FormatErrorKey(kv.Key), kv => kv.Value);

        if (TryGetRequiredValueError(invalidModelState, out var requiredFieldError))
        {
            errors.Add(requiredFieldError);
        }

        if (TryGetInvalidValueError(invalidModelState, out var invalidValueError))
        {
            errors.Add(invalidValueError);
        }

        if (errors.Count == 0)
        {
            return next();
        }

        var problemDetailsFactory = context.HttpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();

        var problemDetails = problemDetailsFactory.CreateValidationProblemDetails(
            context.HttpContext,
            new ModelStateDictionary()
        );

        context.Result = new BadRequestObjectResult(ValidationProblemViewModel.Create(problemDetails, errors));

        return Task.CompletedTask;
    }

    private static List<ErrorViewModel> GetUnknownQueryParameterErrors(ActionExecutingContext context)
    {
        return context
            .HttpContext.Request.Query.Where(param => !context.ModelState.ContainsKey(param.Key))
            .Select(param => new ErrorViewModel
            {
                Code = ValidationMessages.UnknownField.Code,
                Message = ValidationMessages.UnknownField.Message,
                Path = param.Key,
            })
            .ToList();
    }

    private static bool TryGetJsonError(
        IDictionary<string, ModelStateEntry> errorEntries,
        [NotNullWhen(true)] out ErrorViewModel? error
    )
    {
        var modelError = errorEntries
            .SelectMany(entry => entry.Value.Errors)
            .FirstOrDefault(error => error.Exception is JsonException);

        if (modelError is null)
        {
            error = null;
            return false;
        }

        error = GetJsonError(modelError);

        return true;
    }

    private static ErrorViewModel GetJsonError(ModelError error)
    {
        List<string> paths = [];

        var currentException = error.Exception;
        var completed = false;

        while (!completed)
        {
            if (currentException is JsonException { Path: not null } jsonException)
            {
                paths.Add(jsonException.Path);
            }

            if (currentException?.InnerException is not null)
            {
                currentException = currentException.InnerException;
            }
            else
            {
                completed = true;
            }
        }

        // Not ideal, but there isn't any better way of figuring out if we have this type
        // of JsonException as they don't provide any error codes or other identifier.
        var message =
            currentException is not null
            && currentException.Message.StartsWith("The JSON property")
            && currentException.Message.Contains("could not be mapped to any .NET member contained in type")
                ? ValidationMessages.UnknownField
                : ValidationMessages.InvalidValue;

        return new ErrorViewModel
        {
            Code = message.Code,
            Message = message.Message,
            Path = JsonPathUtils.Concat(paths),
        };
    }

    private static bool TryGetEmptyBodyError(
        IDictionary<string, ModelStateEntry> modelState,
        [NotNullWhen(true)] out ErrorViewModel? error
    )
    {
        if (
            !modelState.Any(entry =>
                entry.Value.Errors.Any(e => e.ErrorMessage == ValidationMessages.NotEmptyBody.Message)
            )
        )
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

    private static bool TryGetRequiredValueError(
        IDictionary<string, ModelStateEntry> modelState,
        [NotNullWhen(true)] out ErrorViewModel? error
    )
    {
        var requiredFieldErrorKey = modelState
            // This isn't the ideal way of determining if we have a required field error.
            // Despite trying a few things (including using localization files), there didn't
            // seem to be an easy way to change the default error message for `RequiredAttribute`.
            // In the interest of time, this approach will have to do.
            // TODO: Work out better way to modify default data annotation error messages
            .FirstOrDefault(entry =>
                entry.Value.Errors.Any(e =>
                    e.ErrorMessage.StartsWith("The") && e.ErrorMessage.EndsWith("field is required.")
                )
            )
            .Key;

        if (requiredFieldErrorKey.IsNullOrEmpty())
        {
            error = null;
            return false;
        }

        error = new ErrorViewModel
        {
            Code = ValidationMessages.RequiredValue.Code,
            Message = ValidationMessages.RequiredValue.Message,
            Path = requiredFieldErrorKey,
        };

        return true;
    }

    private static bool TryGetInvalidValueError(
        IDictionary<string, ModelStateEntry> modelState,
        [NotNullWhen(true)] out ErrorViewModel? error
    )
    {
        var invalidValueErrorKey = modelState
            .FirstOrDefault(entry =>
                entry.Value.Errors.Any(e => e.ErrorMessage == ValidationMessages.InvalidValue.Message)
            )
            .Key;

        if (invalidValueErrorKey.IsNullOrEmpty())
        {
            error = null;
            return false;
        }

        error = new ErrorViewModel
        {
            Code = ValidationMessages.InvalidValue.Code,
            Message = ValidationMessages.InvalidValue.Message,
            Path = invalidValueErrorKey,
        };

        return true;
    }

    private static string FormatErrorKey(string errorKey)
    {
        if (errorKey.IsNullOrEmpty())
        {
            return errorKey;
        }

        // Assume that the error key is camelCased already
        if (errorKey[0] != '$' && !char.IsUpper(errorKey[0]))
        {
            return errorKey;
        }

        // The error key is PascalCased, so we have to camelCase it
        var parts = errorKey.Split('.');

        return parts.Select(part => part.ToLowerFirst()).JoinToString('.');
    }
}
