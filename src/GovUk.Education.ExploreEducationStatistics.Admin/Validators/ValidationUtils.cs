using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
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
            return new ValidationResult(message.ToString().ScreamingSnakeCase());
        }
        
        public static Either<ValidationResult, T> ValidationResult<T>(ValidationErrorMessages message)
        {
            return ValidationResult(message);
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
        ContentBlockNotFound,
        IncorrectContentBlockTypeForUpdate,
        ContentBlockAlreadyAttachedToContentSection,
        IncorrectContentBlockTypeForAttach,
        ContentBlockAlreadyDetached,
        ContentBlockNotAttachedToThisContentSection
    }
}