using System.ComponentModel.DataAnnotations;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;
using ValidationResult = System.ComponentModel.DataAnnotations.ValidationResult;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Validators
{
    public class PartialDateValidator : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var partialDate = (PartialDate)value;
            if (partialDate == null || partialDate.IsEmpty() || partialDate.IsValid())
            {
                return ValidationResult.Success;
            }

            return ValidationResult(PartialDateNotValid);
        }
    }
}