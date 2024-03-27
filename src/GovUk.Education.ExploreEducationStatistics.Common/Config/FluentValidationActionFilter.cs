#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GovUk.Education.ExploreEducationStatistics.Common.Config;

/// <summary>
/// <para>
/// A lot of this has been cribbed from the SharpGrip.FluentValidation.AutoValidation package.
/// It has been tidied up and importantly provides support for more details in validation errors.
/// </para>
/// <para>
/// We may want to swap back to SharpGrip's implementation once it supports the
/// <a href="https://github.com/SharpGrip/FluentValidation.AutoValidation/issues/12">required functionality</a>.
/// </para>
/// </summary>
public class FluentValidationActionFilter : IAsyncActionFilter
{
    private static readonly HashSet<string> IgnoredMessagePlaceholders = new()
    {
        "PropertyName",
        "PropertyPath",
        "PropertyValue",
        "CollectionIndex"
    };

    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        if (context.Controller is not ControllerBase controllerBase)
        {
            await next();

            return;
        }

        var errors = new List<ErrorViewModel>();

        foreach (var parameter in context.ActionDescriptor.Parameters)
        {
            if (!context.ActionArguments.TryGetValue(parameter.Name, out var subject))
            {
                continue;
            }

            if (subject == null || !IsValidParameter(parameter))
            {
                continue;
            }

            if (GetValidator(context.HttpContext.RequestServices, parameter) is not IValidator validator)
            {
                continue;
            }

            var validationContext = new ValidationContext<object>(subject);

            var validationResult = await validator.ValidateAsync(
                validationContext,
                context.HttpContext.RequestAborted
            );

            if (validationResult.IsValid)
            {
                continue;
            }

            errors.AddRange(
                validationResult.Errors.Select(failure => CreateErrorViewModel(failure))
            );
        }

        if (errors.Count != 0)
        {
            var details = controllerBase.ProblemDetailsFactory.CreateValidationProblemDetails(
                context.HttpContext,
                new ModelStateDictionary()
            );

            context.Result = new BadRequestObjectResult(ValidationProblemViewModel.Create(details, errors));

            return;
        }

        await next();
    }

    private static object? GetValidator(IServiceProvider serviceProvider, ParameterDescriptor parameter)
    {
        return serviceProvider.GetService(typeof(IValidator<>).MakeGenericType(parameter.ParameterType));
    }

    private static bool IsValidParameter(ParameterDescriptor parameter)
    {
        var type = parameter.ParameterType;
        var bindingSource = parameter.BindingInfo?.BindingSource;

        return type is { IsClass: true, IsPrimitive: false, IsEnum: false, IsValueType: false }
               && HasValidBindingSource(bindingSource);
    }

    private static bool HasValidBindingSource(BindingSource? bindingSource)
    {
        return bindingSource == BindingSource.Body ||
               bindingSource == BindingSource.Form ||
               bindingSource == BindingSource.Query;
    }

    private static ErrorViewModel CreateErrorViewModel(ValidationFailure failure)
    {
        var detail = GetErrorDetail(failure);

        return new ErrorViewModel
        {
            Path = failure.PropertyName,
            Code = failure.ErrorCode.Replace("Validator", ""),
            Message = failure.ErrorMessage,
            Detail = detail.Count > 0 ? detail : null
        };
    }

    private static IDictionary<string, object?> GetErrorDetail(ValidationFailure failure)
    {
        var detail = (failure.FormattedMessagePlaceholderValues ?? [])
            .Where(kv => !IgnoredMessagePlaceholders.Contains(kv.Key))
            .ToDictionary(kv => kv.Key.ToLowerFirst(), kv => kv.Value)
            as Dictionary<string, object?>;

        if (!detail.ContainsKey("value"))
        {
            detail["value"] = failure.AttemptedValue;
        }

        if (failure.CustomState is null)
        {
            return detail;
        }

        var type = failure.CustomState.GetType();

        if (type is { IsClass: true, IsPrimitive: false, IsEnum: false, IsValueType: false })
        {
            var customKeyValues = failure.CustomState
                .ToDictionary()
                .ToDictionary(
                    kv => kv.Key.CamelCase(),
                    kv => kv.Value);

            foreach (var keyValue in customKeyValues)
            {
                detail[keyValue.Key] = keyValue.Value;
            }
        }
        else
        {
            detail["state"] = failure.CustomState;
        }

        return detail;
    }
}
