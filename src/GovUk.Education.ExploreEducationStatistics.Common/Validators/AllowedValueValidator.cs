#nullable enable
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Services;

namespace GovUk.Education.ExploreEducationStatistics.Common.Validators;

public static class AllowedValueValidator
{
    public static IRuleBuilderOptions<T, TProperty> AllowedValue<T, TProperty>(
        this IRuleBuilder<T, TProperty> rule,
        IEnumerable<TProperty> allowedValues)
    {
        var allowed = allowedValues.ToHashSet();

        return rule
            .Must((_, value) => allowed.Contains(value))
            .WithError(ValidationErrorMessages.AllowedValue, usePluralMessage: false)
            .WithState((_, value) => GetErrorDetail(values: CollectionUtils.ListOf(value), allowed: allowed));
    }

    public static IRuleBuilderOptions<T, ICollection<TProperty>> OnlyAllowedValues<T, TProperty>(
        this IRuleBuilder<T, ICollection<TProperty>> rule,
        IEnumerable<TProperty> allowedValues)
    {
        var allowed = allowedValues.ToHashSet();

        return rule
            .Must((_, values) => values.All(allowed.Contains))
            .WithError(ValidationErrorMessages.AllowedValue)
            .WithState((_, values) => GetErrorDetail(values: values, allowed: allowed));
    }

    public static IRuleBuilderOptions<T, IReadOnlyCollection<TProperty>> OnlyAllowedValues<T, TProperty>(
        this IRuleBuilder<T, IReadOnlyCollection<TProperty>> rule,
        IEnumerable<TProperty> allowedValues)
    {
        var allowed = allowedValues.ToHashSet();

        return rule
            .Must((_, values) => values.All(allowed.Contains))
            .WithError(ValidationErrorMessages.AllowedValue)
            .WithState((_, values) => GetErrorDetail(values: values, allowed: allowed));
    }

    private static ErrorDetail<T> GetErrorDetail<T>(IEnumerable<T> values, IEnumerable<T> allowed)
    {
        return new ErrorDetail<T>(
            Invalid: values.Where(value => !allowed.Contains(value)).ToList(),
            Allowed: allowed.OrderBy(v => v).ToList()
        );
    }

    public record ErrorDetail<T>(IEnumerable<T> Invalid, IEnumerable<T> Allowed) : InvalidItemsErrorDetail<T>(Invalid);
}
