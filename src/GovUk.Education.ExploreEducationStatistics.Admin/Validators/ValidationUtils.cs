using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Validators
{
    public static class ValidationUtils
    {
        public static void AddErrors(ModelStateDictionary errors, ValidationResult result)
        {
            foreach (var memberName in result.MemberNames)
            {
                errors.AddModelError(memberName, result.ErrorMessage);
            }
        }

        public static ValidationResult ValidationResult(string memberName, string errorMessage)
        {
            return new ValidationResult(errorMessage, new List<string> {memberName});
        }
    }
}