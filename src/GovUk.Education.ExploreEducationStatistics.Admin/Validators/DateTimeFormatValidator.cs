using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Validators
{
    /**
     * Check that a string can be parsed with the specified DateTime format.
     */
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    public class DateTimeFormatValidator : ValidationAttribute
    {
        public DateTimeFormatValidator(string format)
        {
            Format = format;
        }

        private string Format { get; }

        private string GetErrorMessage() => $"Invalid date format. Expected {Format}.";

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var stringValue = (string) value;

            if (stringValue == null || stringValue == "") {
                return ValidationResult.Success;
            }

            return DateTime.TryParseExact(stringValue, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out _)
                ? ValidationResult.Success
                : new ValidationResult(GetErrorMessage());
        }
    }
}