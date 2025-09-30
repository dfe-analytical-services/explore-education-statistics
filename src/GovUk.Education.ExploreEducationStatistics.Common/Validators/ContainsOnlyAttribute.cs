#nullable enable
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;

namespace GovUk.Education.ExploreEducationStatistics.Common.Validators;

[AttributeUsage(
    AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter,
    AllowMultiple = true)]
public class ContainsOnlyAttribute(params object[] allowedValues) : ValidationAttribute
{
    private IEnumerable<object> AllowedValues { get; } = allowedValues;

    public string? AllowedValuesProvider { get; set; }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var allowedValues = GetAllowedValues(validationContext);

        if (value is not IEnumerable values)
        {
            throw new ArgumentException(
                $"Validated value must implement {nameof(IEnumerable)}",
                // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
                validationContext?.MemberName
            );
        }

        if (!allowedValues.Any())
        {
            return ValidationResult.Success;
        }

        if (!values.Cast<object>().All(allowedValues.Contains))
        {
            return new ValidationResult(
                ErrorMessage ?? $"Field must contain only values from: {allowedValues.JoinToString(", ")}");
        }

        return ValidationResult.Success;
    }

    private HashSet<object> GetAllowedValues(ValidationContext validationContext)
    {
        if (AllowedValuesProvider is null)
        {
            return AllowedValues.ToHashSet();
        }

        var instance = validationContext.ObjectInstance;
        var type = instance.GetType();
        var member = type.GetMember(
                AllowedValuesProvider,
                MemberTypes.Field | MemberTypes.Property | MemberTypes.Method,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static
            )
            .FirstOrDefault()
                ?? throw new ArgumentException(
                    $"{nameof(AllowedValuesProvider)} must reference a field or method in {type.FullName}",
                    AllowedValuesProvider
                );

        var providedValues = member.MemberType switch
        {
            MemberTypes.Field => ((FieldInfo)member).GetValue(instance),
            MemberTypes.Property => ((PropertyInfo)member).GetValue(instance),
            MemberTypes.Method => ((MethodInfo)member).Invoke(instance, []),
            _ => throw new ArgumentOutOfRangeException(
                $"{nameof(AllowedValuesProvider)} is not a valid member on {type.FullName}",
                AllowedValuesProvider
            )
        };

        if (providedValues is not IEnumerable<object> providedAllowedValues)
        {
            throw new ArgumentException(
                $"{nameof(AllowedValuesProvider)} on {type.FullName} must implement {nameof(IEnumerable)}",
                AllowedValuesProvider
            );
        }

        return AllowedValues
            .Concat(providedAllowedValues)
            .ToHashSet();
    }
}
