#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Admin.Validators;

public enum ValidationErrorMessages
{
    // Slug
    SlugNotUnique,
    PublicationSlugNotUnique,
    PublicationSlugUsedByRedirect,
    MethodologySlugNotUnique,
    MethodologySlugUsedByRedirect,

    // Partial date
    PartialDateNotValid,

    // Content
    IncorrectContentBlockTypeForUpdate,
    ContentBlockAlreadyAttachedToContentSection,
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
    MethodologyCannotDependOnPublishedRelease,
    MethodologyCannotDependOnRelease,
    CannotAdoptMethodologyAlreadyLinkedToPublication,

    // Methodology published update
    MethodologyPublishedCannotBeFutureDate,
    MethodologyNotPublished,

    // Theme
    ThemeDoesNotExist,

    // Topic
    TopicDoesNotExist,

    // File
    CannotOverwriteFile,
    FileCannotBeEmpty,
    FileTypeInvalid,
    FileSizeLimitExceeded,

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
    DataFilenameTooLong,

    // Data zip file
    DataZipMustBeZipFile,
    DataZipFileCanOnlyContainTwoFiles,
    DataZipFileDoesNotContainCsvFiles,
    DataZipFilenameTooLong,
    DataZipContentFilenamesTooLong,

    // Meta file
    MetadataFileCannotBeEmpty,
    MetaFileMustBeCsvFile,
    UnableToFindMetadataFileToDelete,
    MetaFilenameCannotContainSpacesOrSpecialCharacters,
    MetaFileIsIncorrectlyNamed,
    MetaFilenameTooLong,

    // Data replacement
    ReplacementMustBeValid,

    // Release
    ReleaseTypeInvalid,

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
    EmptyContentSectionExists,
    GenericSectionsContainEmptyHtmlBlock,
    RelatedDashboardsSectionContainsEmptyHtmlBlock,
    ReleaseMustContainKeyStatOrNonEmptyHeadlineBlock,
    SummarySectionContainsEmptyHtmlBlock,

    // Data guidance
    DataGuidanceDataSetNotAttachedToRelease,

    // Release checklist warnings
    NoMethodology,
    NoNextReleaseDate,
    NoDataFiles,
    NoFootnotesOnSubjects,
    NoFeaturedTables,
    NoPublicPreReleaseAccessList,
    MethodologyNotApproved,

    // Footnotes
    FootnotesDifferFromReleaseFootnotes,

    // Key statistics
    DataBlockShouldBeUnattached,
    ProvidedKeyStatIdsDifferFromReleaseKeyStatIds,

    // Featured tables
    DataBlockAlreadyHasFeaturedTable,
    ProvidedFeaturedTableIdsDifferFromReleaseFeaturedTableIds,
}
