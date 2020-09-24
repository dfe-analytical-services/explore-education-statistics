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
        // Slug
        SlugNotUnique,

        // Partial date
        PartialDateNotValid,

        // Content blocks
        ContentBlockNotFound,
        IncorrectContentBlockTypeForUpdate,
        ContentBlockAlreadyAttachedToContentSection,
        IncorrectContentBlockTypeForAttach,
        ContentBlockAlreadyDetached,
        ContentBlockNotAttachedToThisContentSection,

        // User
        UserAlreadyExists,
        UserDoesNotExist,
        UserAlreadyHasReleaseRole,

        // Role
        RoleDoesNotExist,

        // Invite
        UnableToCancelInvite,
        InvalidEmailAddress,

        // Publication
        PublicationDoesNotExist,
        PublicationHasMethodologyAssigned,

        // Methodology
        MethodologyDoesNotExist,
        MethodologyMustBeApprovedOrPublished,
        CannotSpecifyMethodologyAndExternalMethodology,
        MethodologyOrExternalMethodologyLinkMustBeDefined,

        // Theme
        ThemeDoesNotExist,

        // Topic
        TopicDoesNotExist,

        // File
        CannotOverwriteFile,
        FileNotFound,
        FileCannotBeEmpty,
        FileTypeInvalid,
        FilenameCannotContainSpacesOrSpecialCharacters,
        FileUploadNameCannotContainSpecialCharacters,

        // Data file
        SubjectTitleCannotContainSpecialCharacters,
        SubjectTitleMustBeUnique,
        CannotUseGenericFunctionToDeleteDataFile,
        CannotUseGenericFunctionToAddDataFile,
        CannotOverwriteDataFile,
        DataAndMetadataFilesCannotHaveTheSameName,
        DataFileCannotBeEmpty,
        DataFileMustBeCsvFile,
        DataFileAlreadyUploaded,
        DataFilenameCannotContainSpacesOrSpecialCharacters,
        CannotRemoveDataFilesUntilImportComplete,
        CannotRemoveDataFilesOnceReleaseApproved,
        AllDatafilesUploadedMustBeComplete,
        FileTypeMustBeData,

        // Data zip file
        DataFileMustBeZipFile,
        DataZipFileCanOnlyContainTwoFiles,
        DataZipFileDoesNotContainCsvFiles,
        DataZipFileAlreadyExists,

        ReplacementFileTypesMustBeData,
        ReplacementMustBeValid,

        // Meta file
        CannotOverwriteMetadataFile,
        MetadataFileCannotBeEmpty,
        MetaFileMustBeCsvFile,
        UnableToFindMetadataFileToDelete,
        MetaFilenameCannotContainSpacesOrSpecialCharacters,
        MetaFileIsIncorrectlyNamed,

        // Release
        ReleaseNotApproved,
        ApprovedReleaseMustHavePublishScheduledDate,
        PublishedReleaseCannotBeUnapproved
    }
}