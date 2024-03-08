#nullable enable
using FluentValidation;
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
}
