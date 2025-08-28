#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ManageContent;

public record ContentBlockLockViewModel(
    Guid Id,
    Guid SectionId,
    Guid ReleaseVersionId,
    DateTimeOffset Locked,
    DateTimeOffset LockedUntil,
    UserDetailsViewModel LockedBy);
