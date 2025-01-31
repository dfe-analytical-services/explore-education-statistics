#nullable enable
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;

namespace GovUk.Education.ExploreEducationStatistics.Common.Validators;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field| AttributeTargets.Parameter)]
public class PhoneNumberAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
        {
            return ValidationResult.Success;
        }

        if (value is not string telNo)
        {
            return new ValidationResult("A phone number must be a string");
        }

        var trimmedTelNo = telNo.Trim();

        if (trimmedTelNo.IsNullOrWhitespace())
        {
            return new ValidationResult("A phone number cannot be an empty string or whitespace");
        }

        if (trimmedTelNo.Length < 8)
        {
            return new ValidationResult(LengthMessage(validationContext));
        }

        return !Regex.IsMatch(trimmedTelNo, @"^0[0-9\s]*$")
            ? new ValidationResult(FormatMessage(validationContext))
            : ValidationResult.Success;
    }

    private string LengthMessage(ValidationContext validationContext) =>
        validationContext?.MemberName is not null
            ? $"The {validationContext.MemberName} field must have a minimum length of 8."
            : "The value must have a minimum length of 8.";

    private string FormatMessage(ValidationContext validationContext) =>
        validationContext?.MemberName is not null
            ? $"The {validationContext.MemberName} field must start with a '0' and contain only numbers or spaces."
            : "The value must start with a '0' and  contain only numbers or spaces.";
}
