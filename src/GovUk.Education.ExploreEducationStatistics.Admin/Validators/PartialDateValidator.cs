using System.ComponentModel.DataAnnotations;
using GovUk.Education.ExploreEducationStatistics.Content.Model;


namespace GovUk.Education.ExploreEducationStatistics.Admin.Validators
{
    public class PartialDateValidator : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var partialDate = (PartialDate)validationContext.ObjectInstance;
            if (partialDate == null || partialDate.IsValid())
            {
                return ValidationResult.Success;
            }
            return new ValidationResult("Partial date not valid");
        }
    }
}