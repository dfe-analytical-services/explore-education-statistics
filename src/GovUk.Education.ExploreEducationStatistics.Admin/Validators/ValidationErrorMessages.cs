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
        ContentSectionNotAttachedToRelease,
        ContentBlockNotAttachedToRelease,
        EmbedBlockUrlDomainNotPermitted,

        // User Management
        UserAlreadyExists,
        UserAlreadyHasResourceRole,

        // Invite
        InviteAlreadyAccepted,
        InviteNotFound,
        InvalidEmailAddress,
        InvalidUserRole,
        NoInvitableEmails,
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
        SubjectTitleCannotBeEmpty,
        SubjectTitleCannotContainSpecialCharacters,
        SubjectTitleMustBeUnique,
        DataFilenameNotUnique,
        DataAndMetadataFilesCannotHaveTheSameName,
        DataFileCannotBeEmpty,
        DataFileMustBeCsvFile,
        DataFilenameCannotContainSpacesOrSpecialCharacters,
        CannotRemoveDataFilesUntilImportComplete,
        CannotRemoveDataFilesOnceReleaseApproved,
        FileTypeMustBeData,
        FileIdsShouldBeDistinct,
        IncorrectNumberOfFileIds,

        // Data zip file
        DataZipMustBeZipFile,
        DataZipFileCanOnlyContainTwoFiles,
        DataZipFileDoesNotContainCsvFiles,

        // Meta file
        MetadataFileCannotBeEmpty,
        MetaFileMustBeCsvFile,
        UnableToFindMetadataFileToDelete,
        MetaFilenameCannotContainSpacesOrSpecialCharacters,
        MetaFileIsIncorrectlyNamed,

        // Data replacement
        ReplacementFileTypesMustBeData,
        ReplacementMustBeValid,

        // Release approval
        ReleaseNotApproved,
        PublishedReleaseCannotBeUnapproved,
        PublishDateCannotBeEmpty,
        PublishDateCannotBeScheduled,

        // Release update
        ReleasePublishedCannotBeFutureDate,
        ReleaseNotPublished,

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
        MethodologyNotApproved,

        // Footnotes
        FootnotesDifferFromReleaseFootnotes,

        // Key statistics
        DataBlockShouldBeUnattached,
        ProvidedKeyStatIdsDifferFromReleaseKeyStatIds,

        // TODO EES-3755 Remove after Permalink snapshot migration work is complete
        NullMessageCountForPermalinksMigrationQueue,
        NonEmptyPermalinksMigrationQueue,
    }
}
