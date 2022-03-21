#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Admin.Validators
{
    public enum ValidationErrorMessages
    {
        // Slug
        SlugNotUnique,

        // Partial date
        PartialDateNotValid,

        // Content
        EmptyContentSectionExists,
        GenericSectionsContainEmptyHtmlBlock,
        ContentBlockNotFound,
        IncorrectContentBlockTypeForUpdate,
        ContentBlockAlreadyAttachedToContentSection,
        IncorrectContentBlockTypeForAttach,
        ContentBlockAlreadyDetached,
        ContentBlockNotAttachedToThisContentSection,

        // User Management
        UserAlreadyExists,
        UserAlreadyHasResourceRole,

        // Invite
        InviteNotFound,
        InvalidEmailAddress,
        NoInvitableEmails,
        InvalidUserRole,
        NotAllReleasesBelongToPublication,
        UserAlreadyHasReleaseRoleInvites,
        UserAlreadyHasReleaseRoles,

        // Methodology
        MethodologyMustBeDraft,
        MethodologyCannotDependOnPublishedRelease,
        MethodologyCannotDependOnRelease,
        CannotAdoptMethodologyAlreadyLinkedToPublication,

        // Theme
        ThemeDoesNotExist,

        // Topic
        TopicDoesNotExist,

        // File
        CannotOverwriteFile,
        FileCannotBeEmpty,
        FileTypeInvalid,

        // Data file
        SubjectTitleCannotContainSpecialCharacters,
        SubjectTitleMustBeUnique,
        CannotOverwriteDataFile,
        DataAndMetadataFilesCannotHaveTheSameName,
        DataFileCannotBeEmpty,
        DataFileMustBeCsvFile,
        DataFilenameCannotContainSpacesOrSpecialCharacters,
        CannotRemoveDataFilesUntilImportComplete,
        CannotRemoveDataFilesOnceReleaseApproved,
        FileTypeMustBeData,

        // Data zip file
        DataFileMustBeZipFile,
        DataZipFileCanOnlyContainTwoFiles,
        DataZipFileDoesNotContainCsvFiles,

        ReplacementFileTypesMustBeData,
        ReplacementMustBeValid,

        // Meta file
        MetadataFileCannotBeEmpty,
        MetaFileMustBeCsvFile,
        UnableToFindMetadataFileToDelete,
        MetaFilenameCannotContainSpacesOrSpecialCharacters,
        MetaFileIsIncorrectlyNamed,

        // Release
        ReleaseNotApproved,
        ApprovedReleaseMustHavePublishScheduledDate,
        PublishedReleaseCannotBeUnapproved,

        // Release checklist errors
        DataFileImportsMustBeCompleted,
        DataFileReplacementsMustBeCompleted,
        ReleaseNoteRequired,
        PublicDataGuidanceRequired,

        // Release checklist warnings
        NoMethodology,
        NoNextReleaseDate,
        NoDataFiles,
        NoFootnotesOnSubjects,
        NoTableHighlights,
        NoPublicPreReleaseAccessList,
        MethodologyNotApproved
    }
}
