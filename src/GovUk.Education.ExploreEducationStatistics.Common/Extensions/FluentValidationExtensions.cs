#nullable enable
using FluentValidation;
using FluentValidation.Results;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Common.Extensions;

public static class FluentValidationRuleExtensions
{
    public static async Task<Either<ActionResult, T>> Validate<T>(
        this IValidator<T> validator,
        T instance,
        CancellationToken cancellationToken = default
    )
    {
        var result = await validator.ValidateAsync(instance, cancellationToken);

        if (result.IsValid)
        {
            return instance;
        }

        return ValidationUtils.ValidationResult(result.Errors.Select(ErrorViewModel.Create));
    }

    public static IRuleBuilderOptions<T, TProperty> WithMessage<T, TProperty>(
        this IRuleBuilderOptions<T, TProperty> rule,
        LocalizableMessage localizableMessage
    )
    {
        return rule.WithErrorCode(localizableMessage.Code).WithMessage(localizableMessage.Message);
    }

    public static IRuleBuilderOptions<T, TProperty> WithMessage<T, TProperty>(
        this IRuleBuilderOptions<T, TProperty> rule,
        LocalizableMessage localizableMessage,
        params object[] value
    )
    {
        return rule.WithErrorCode(localizableMessage.Code)
            .WithMessage(string.Format(localizableMessage.Message, value));
    }

    public static void AddFailure<T, TDetail>(
        this ValidationContext<T> context,
        LocalizableMessage message,
        TDetail? detail,
        Dictionary<string, object>? messagePlaceholders = null
    )
        where TDetail : class
    {
        context.AddFailure(
            new ValidationFailure
            {
                PropertyName = context.PropertyPath,
                ErrorCode = message.Code,
                ErrorMessage = message.Message,
                CustomState = detail,
                FormattedMessagePlaceholderValues = messagePlaceholders,
            }
        );
    }
}
