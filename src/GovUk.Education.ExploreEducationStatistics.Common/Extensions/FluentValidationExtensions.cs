#nullable enable
using System.Collections.Generic;
using FluentValidation;
using FluentValidation.Results;
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Common.Extensions;

public static class FluentValidationRuleExtensions
{
    public static IRuleBuilderOptions<T, TProperty> WithMessage<T, TProperty>(
        this IRuleBuilderOptions<T, TProperty> rule,
        LocalizableMessage localizableMessage)
    {
        return rule
            .WithErrorCode(localizableMessage.Code)
            .WithMessage(localizableMessage.Message);
    }

    public static void AddFailure<T, TDetail>(
        this ValidationContext<T> context,
        LocalizableMessage message,
        TDetail? detail,
        Dictionary<string, object>? messagePlaceholders = null) where TDetail : class
    {
        context.AddFailure(new ValidationFailure
        {
            PropertyName = context.PropertyPath,
            ErrorCode = message.Code,
            ErrorMessage = message.Message,
            CustomState = detail,
            FormattedMessagePlaceholderValues = messagePlaceholders
        });
    }
}
