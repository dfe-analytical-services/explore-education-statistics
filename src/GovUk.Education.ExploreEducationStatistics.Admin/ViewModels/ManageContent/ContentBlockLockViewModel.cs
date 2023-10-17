#nullable enable
using System;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ManageContent;

public record ContentBlockLockViewModel(
    Guid Id,
    Guid SectionId,
    Guid ReleaseId,
    DateTimeOffset Locked,
    DateTimeOffset LockedUntil,
    UserDetailsViewModel LockedBy);