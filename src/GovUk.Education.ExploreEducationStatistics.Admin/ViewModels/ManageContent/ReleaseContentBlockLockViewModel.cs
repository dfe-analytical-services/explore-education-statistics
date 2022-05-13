#nullable enable
using System;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ManageContent;

public record ReleaseContentBlockLockViewModel
{
    public Guid Id { get; }

    public Guid SectionId { get; }

    public Guid ReleaseId { get; }
    public DateTimeOffset Locked { get; }

    public DateTimeOffset LockedUntil { get; }

    public UserDetailsViewModel LockedBy { get; }

    public ReleaseContentBlockLockViewModel(
        Guid id,
        Guid sectionId,
        Guid releaseId,
        DateTime locked,
        DateTime lockedUntil,
        UserDetailsViewModel lockedBy)
    {
        Id = id;
        SectionId = sectionId;
        ReleaseId = releaseId;
        Locked = locked;
        LockedUntil = lockedUntil;
        LockedBy = lockedBy;
    }
}