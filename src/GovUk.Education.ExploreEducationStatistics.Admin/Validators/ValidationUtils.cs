using System.ComponentModel.DataAnnotations;
using System.Linq;
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
        public static ValidationResult ValidationResult(ValidationErrorMessages message)
        {
            return new ValidationResult(message.ToString().ScreamingSnakeCase());
        }
        
        // TODO EES-919 - return ActionResults rather than ValidationResults - as this work is done,
        // rename this to "ValidationResult"
        public static ActionResult ValidationActionResult(ValidationErrorMessages message)
        {
            ModelStateDictionary errors = new ModelStateDictionary();
            errors.AddModelError(string.Empty, message.ToString().ScreamingSnakeCase());
            return new BadRequestObjectResult(new ValidationProblemDetails(errors));
        }

        // TODO EES-919 - return ActionResults rather than ValidationResults - as this work is done,
        // rename this to "ValidationResult"
        public static Either<ActionResult, T> ValidationActionResult<T>(ValidationErrorMessages message)
        {
            return ValidationActionResult(message);
        }

        public static Either<ActionResult, T> NotFound<T>()
        {
            return new NotFoundResult();
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
        FileTypeInvalid,
        SubjectTitleMustBeUnique,
        ContentBlockNotFound,
        IncorrectContentBlockTypeForUpdate,
        ContentBlockAlreadyAttachedToContentSection,
        IncorrectContentBlockTypeForAttach,
        ContentBlockAlreadyDetached,
        ContentBlockNotAttachedToThisContentSection,
        CommentNotFound,
        UserAlreadyExists,
        UserDoesNotExist,
        RoleDoesNotExist,
        UnableToCancelInvite,
        InvalidEmailAddress,
        ApprovedReleaseMustHavePublishScheduledDate,
        PublicationDoesNotExist,
        PublicationHasMethodologyAssigned,
        MethodologyDoesNotExist, 
        MethodologyMustBeApprovedOrPublished,
        CannotSpecifyMethodologyAndExternalMethodology,
        MethodologyOrExternalMethodologyLinkMustBeDefined,
        CannotRemoveDataFilesUntilImportComplete,
        DatafileAlreadyUploaded,
        AllDatafilesUploadedMustBeComplete,
        ReleaseNotApproved
    }
}