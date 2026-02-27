using System.Text.Json;
using FluentValidation;

namespace GovUk.Education.ExploreEducationStatistics.Common.Extensions;

public static class ValidatorExtensions
{
    public static IRuleBuilderOptions<T, string?> IsValidJson<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .Must(jsonStr =>
            {
                if (string.IsNullOrWhiteSpace(jsonStr))
                    return false;
                try
                {
                    using var jsonDoc = JsonDocument.Parse(jsonStr);
                    return true;
                }
                catch (JsonException)
                {
                    return false;
                }
            })
            .WithMessage("{PropertyName} must be a valid JSON string.");
    }

    public static IRuleBuilderOptions<T, string?> IsValidUrl<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
            .WithMessage("{PropertyName} must be a valid URL.");
    }
}
