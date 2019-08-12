using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Validators
{
    public static class ValidationUtils
    {
        public static void AddErrors(ModelStateDictionary errors, ValidationResult result)
        {
            if (result.MemberNames?.Any() ?? false)
            {
                foreach (var memberName in result.MemberNames)
                {
                    errors.AddModelError(memberName, result.ErrorMessage);
                }    
            }
            else
            {
                // This appears to be the default MVC way of reporting generalised validation errors not associated  
                // with a single property.
                errors.AddModelError(string.Empty, result.ErrorMessage);
            }
        }

        public static ValidationResult ValidationResult(ValidationErrorMessages message)
        {
            switch (message)
            {
                case CannotOverwriteFile:
                    return new ValidationResult("CANNOT_OVERWRITE_FILE");
                case SlugNotUnique:
                    return new ValidationResult("SLUG_NOT_UNIQUE");
                case PartialDateNotValid:
                    return new ValidationResult("PARTIAL_DATE_NOT_VALID");
                case CannotUseGenericFunctionToDeleteDataFile:
                    return new ValidationResult("CANNOT_USE_GENERIC_FUNCTION_TO_DELETE_DATA_FILE");
                case UnableToFindMetadataFileToDelete:
                    return new ValidationResult("UNABLE_TO_FIND_METADATA_FILE_TO_DELETE");
                default:
                    throw new ArgumentOutOfRangeException(nameof(message), message, null);
            }
        }
    }

    public enum ValidationErrorMessages
    {
        CannotOverwriteFile,
        SlugNotUnique,
        PartialDateNotValid,
        CannotUseGenericFunctionToDeleteDataFile,
        UnableToFindMetadataFileToDelete
    }
}