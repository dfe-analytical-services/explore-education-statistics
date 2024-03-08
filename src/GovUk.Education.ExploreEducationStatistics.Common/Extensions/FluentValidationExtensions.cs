#nullable enable
using System.Collections.Generic;
using FluentValidation;
using FluentValidation.Results;
using GovUk.Education.ExploreEducationStatistics.Common.Validators;

namespace GovUk.Education.ExploreEducationStatistics.Common.Extensions;

public static class FluentValidationRuleExtensions
{
    public static IRuleBuilderOptions<T, TProperty> WithError<T, TProperty>(
        this IRuleBuilderOptions<T, TProperty> rule,
        ValidationErrorMessage message,
        bool usePluralMessage = true)
    {
        return rule
            .WithErrorCode(message.Code)
            .WithMessage(usePluralMessage ? message.Plural : message.Single);
    }

    public static void AddFailure<T, TDetail>(
        this ValidationContext<T> context,
        ValidationErrorMessage error,
        TDetail? detail,
        Dictionary<string, object>? messagePlaceholders = null,
        bool usePluralMessage = true) where TDetail : class
    {
        context.AddFailure(new ValidationFailure
        {
            PropertyName = context.PropertyPath,
            ErrorCode = error.Code,
            ErrorMessage = usePluralMessage ? error.Plural : error.Single,
            CustomState = detail,
            FormattedMessagePlaceholderValues = messagePlaceholders
        });
    }
}
