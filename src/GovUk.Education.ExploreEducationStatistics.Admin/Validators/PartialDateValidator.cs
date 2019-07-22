using System.ComponentModel.DataAnnotations;
using GovUk.Education.ExploreEducationStatistics.Content.Model;


namespace GovUk.Education.ExploreEducationStatistics.Admin.Validators
{
    public class PartialDateValidator : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var partialDate = (string)validationContext.ObjectInstance;
            if (PartialDateUtil.PartialDateValid(partialDate))
            {
                return ValidationResult.Success;
            }
            return new ValidationResult("Must be of the form YYYY-MM-DD where each value can be missing");
        }
    }
}