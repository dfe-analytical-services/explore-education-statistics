#nullable enable
using FluentValidation;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Validators.ErrorDetails;

namespace GovUk.Education.ExploreEducationStatistics.Common.Validators;

public static class AllowedValueValidator
{
    public static IRuleBuilderOptions<T, TProperty> AllowedValue<T, TProperty>(
        this IRuleBuilder<T, TProperty> rule,
        IEnumerable<TProperty> allowedValues
    )
    {
        var allowed = allowedValues.ToHashSet();

        return rule.Must((_, value) => allowed.Contains(value))
            .WithMessage(ValidationMessages.AllowedValue)
            .WithState((_, value) => GetAllowedDetail(value: value, allowed: allowed));
    }

    private static AllowedErrorDetail<T> GetAllowedDetail<T>(T value, IEnumerable<T> allowed)
    {
        return new AllowedErrorDetail<T>(Value: value, Allowed: allowed.OrderBy(v => v).ToList());
    }

    public record AllowedErrorDetail<T>(T Value, IEnumerable<T> Allowed) : InvalidErrorDetail<T>(Value);
}
