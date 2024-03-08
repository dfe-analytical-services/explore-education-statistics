#nullable enable
using FluentValidation;
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
}
