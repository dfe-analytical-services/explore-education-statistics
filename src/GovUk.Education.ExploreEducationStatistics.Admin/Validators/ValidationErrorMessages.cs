namespace GovUk.Education.ExploreEducationStatistics.Admin.Validators;

public enum ValidationErrorMessages
{
    // Title
    TitleNotUnique,

    // Slug
    SlugNotUnique,
    ReleaseSlugUsedByRedirect,
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

    // File
    CannotOverwriteFile,
    FileCannotBeEmpty,
    FileTypeInvalid,
    FileSizeLimitExceeded,

    // Data file
    CannotRemoveDataFilesUntilImportComplete,
    CannotRemoveDataFilesOnceReleaseApproved,
    FileTypeMustBeData,
    FileIdsShouldBeDistinct,
    IncorrectNumberOfFileIds,

    // Data zip file
    DataZipFileCanOnlyContainTwoFiles,
    DataZipFileDoesNotContainCsvFiles,

    // Meta file
    UnableToFindMetadataFileToDelete,

    // Data replacement
    ReplacementMustBeValid,
    ReplacementImportMustBeComplete,

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
    UpdateRequestForPublishedReleaseVersionInvalid,
    ReleaseUndergoingPublishing,

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
    PublicApiDataSetImportsMustBeCompleted,
    PublicApiDataSetCancellationsMustBeResolved,
    PublicApiDataSetFailuresMustBeResolved,
    PublicApiDataSetMappingsMustBeCompleted,

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

    // Education in numbers
    EinProvidedPageIdsDifferFromActualPageIds,
    EinProvidedSectionIdsDifferFromActualSectionIds,
    EinProvidedBlockIdsDifferFromActualBlockIds,
    EinProvidedTileIdsDifferFromActualTileIds,
}
