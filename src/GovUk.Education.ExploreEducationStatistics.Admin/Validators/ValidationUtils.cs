using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

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

        // TODO EES-919 - return ActionResults rather than ValidationResults
        public static Either<ValidationResult, R> HandleValidationErrors<T, R>(
            Func<Either<ValidationResult, T>> validationErrorAction,
            Func<T, Either<ValidationResult, R>> successAction)
        {
            var validationResult = validationErrorAction.Invoke();

            return validationResult.IsRight 
                ? successAction.Invoke(validationResult.Right) 
                : validationResult.Left;
        }
        
        // TODO EES-919 - return ActionResults rather than ValidationResults
        public static async Task<Either<ValidationResult, R>> HandleValidationErrorsAsync<T, R>(
            Func<Task<Either<ValidationResult, T>>> validationErrorAction,
            Func<T, Task<R>> successAction)
        {
            var validationResult = await validationErrorAction.Invoke();

            return validationResult.IsRight 
                ? new Either<ValidationResult, R>(await successAction.Invoke(validationResult.Right)) 
                : validationResult.Left;
        }

        // TODO EES-919 - return ActionResults rather than ValidationResults
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
        
        public static async Task<ActionResult<T>> HandleErrorsAsync<T>(
            Func<Task<Either<ActionResult, T>>> errorsRaisingAction,
            Func<T, ActionResult> onSuccessAction) 
        {
            var result = await errorsRaisingAction.Invoke();

            return result.IsRight ? onSuccessAction.Invoke(result.Right) : result.Left;
        }
        
        public static async Task<Either<ActionResult, R>> HandleErrorsAsync<T, R>(
            Func<Task<Either<ActionResult, T>>> validationErrorAction,
            Func<T, Task<Either<ActionResult, R>>> successAction)
        {
            var validationResult = await validationErrorAction.Invoke();

            if (validationResult.IsLeft)
            {
                return validationResult.Left;
            }

            return await successAction.Invoke(validationResult.Right);
        }

        // TODO EES-919 - return ActionResults rather than ValidationResults
        public static ValidationResult ValidationResult(ValidationErrorMessages message)
        {
            return new ValidationResult(message.ToString().ScreamingSnakeCase());
        }
        
        // TODO EES-919 - return ActionResults rather than ValidationResults
        public static Either<ValidationResult, T> ValidationResult<T>(ValidationErrorMessages message)
        {
            return ValidationResult(message);
        }

        // TODO EES-919 - return ActionResults rather than ValidationResults - as this work is done,
        // rename this to "ValidationResult"
        public static BadRequestObjectResult ValidationActionResult(ValidationErrorMessages message)
        {
            ModelStateDictionary errors = new ModelStateDictionary();
            errors.AddModelError(string.Empty, message.ToString().ScreamingSnakeCase());
            return new BadRequestObjectResult(new ValidationProblemDetails(errors));
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
        ReleaseNoteNotFound,
        RelatedInformationItemNotFound,
        ContentSectionNotFound,
        ContentBlockNotFound,
        IncorrectContentBlockTypeForUpdate,
        ContentBlockAlreadyAttachedToContentSection,
        IncorrectContentBlockTypeForAttach,
        ContentBlockAlreadyDetached,
        ContentBlockNotAttachedToThisContentSection,
        PublicationNotFound
    }
}