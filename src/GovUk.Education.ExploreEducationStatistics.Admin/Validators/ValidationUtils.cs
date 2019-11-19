using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
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

        public static Either<ValidationResult, R> HandleValidationErrors<T, R>(
            Func<Either<ValidationResult, T>> validationErrorAction,
            Func<T, Either<ValidationResult, R>> successAction)
        {
            var validationResult = validationErrorAction.Invoke();

            return validationResult.IsRight 
                ? successAction.Invoke(validationResult.Right) 
                : validationResult.Left;
        }

        public static async Task<Either<ValidationResult, R>> HandleValidationErrorsAsync<T, R>(
            Func<Task<Either<ValidationResult, T>>> validationErrorAction,
            Func<T, R> successAction)
        {
            var validationResult = await validationErrorAction.Invoke();

            return validationResult.IsRight 
                ? new Either<ValidationResult, R>(successAction.Invoke(validationResult.Right)) 
                : validationResult.Left;
        }

        public static async Task<Either<ValidationResult, R>> HandleValidationErrorsAsync<T, R>(
            Func<Task<Either<ValidationResult, T>>> validationErrorAction,
            Func<T, Task<R>> successAction)
        {
            var validationResult = await validationErrorAction.Invoke();

            return validationResult.IsRight 
                ? new Either<ValidationResult, R>(await successAction.Invoke(validationResult.Right)) 
                : validationResult.Left;
        }

        public static async Task<Either<ValidationResult, R>> HandleValidationErrorsAsync<T, R>(
            Func<Task<Either<ValidationResult, T>>> validationErrorAction,
            Func<T, Task<Either<ValidationResult, R>>> successAction)
        {
            var validationResult = await validationErrorAction.Invoke();

            if (validationResult.IsLeft)
            {
                return validationResult.Left;
            }

            return await successAction.Invoke(validationResult.Right);
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
                case FileNotFound:
                    return new ValidationResult("FILE_NOT_FOUND");
                case FileCannotBeEmpty: 
                    return new ValidationResult("FILE_CAN_NOT_BE_EMPTY");
                case DataFileCannotBeEmpty: 
                    return new ValidationResult("DATA_FILE_CAN_NOT_BE_EMPTY");
                case MetadataFileCannotBeEmpty: 
                    return new ValidationResult("METADATA_FILE_CAN_NOT_BE_EMPTY");
                case CannotUseGenericFunctionToAddDataFile: 
                    return new ValidationResult("CANNOT_USE_GENERIC_FUNCTION_TO_ADD_DATA_FILE");
                case CannotOverwriteDataFile: 
                    return new ValidationResult("CANNOT_OVERWRITE_DATA_FILE");
                case CannotOverwriteMetadataFile: 
                    return new ValidationResult("CANNOT_OVERWRITE_METADATA_FILE");
                case DataAndMetadataFilesCannotHaveTheSameName: 
                    return new ValidationResult("DATA_AND_METADATA_FILES_CANNOT_HAVE_THE_SAME_NAME");
                case DataFileMustBeCsvFile: 
                    return new ValidationResult("DATA_FILE_MUST_BE_A_CSV_FILE");
                case MetaFileMustBeCsvFile: 
                    return new ValidationResult("META_FILE_MUST_BE_A_CSV_FILE");
                case SubjectTitleMustBeUnique: 
                    return new ValidationResult("SUBJECT_TITLE_MUST_BE_UNIQUE");
                case ReleaseNotFound:
                    return new ValidationResult("RELEASE_NOT_FOUND");
                case RelatedInformationItemNotFound:
                    return new ValidationResult("RELATED_INFORMATION_ITEM_NOT_FOUND");
                case EntityNotFound:
                    return new ValidationResult("NOT_FOUND");
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
        CannotUseGenericFunctionToAddDataFile,
        UnableToFindMetadataFileToDelete,
        FileNotFound,
        FileCannotBeEmpty,
        DataFileCannotBeEmpty,
        MetadataFileCannotBeEmpty,
        CannotOverwriteDataFile,
        CannotOverwriteMetadataFile,
        DataAndMetadataFilesCannotHaveTheSameName,
        DataFileMustBeCsvFile,
        MetaFileMustBeCsvFile,
        SubjectTitleMustBeUnique,
        EntityNotFound,
        ReleaseNotFound,
        RelatedInformationItemNotFound,
        ContentSectionNotFound,
    }
}