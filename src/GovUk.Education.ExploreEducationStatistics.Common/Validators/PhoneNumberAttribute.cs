#nullable enable
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

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

        if (telNo.Length < 8)
        {
            return new ValidationResult(LengthMessage(validationContext));
        }

        return !Regex.IsMatch(telNo, @"^0[0-9\s]*$")
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