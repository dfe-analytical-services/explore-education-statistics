#nullable enable
using System;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ManageContent;

public record ReleaseContentBlockUnlockViewModel
{
    public Guid Id { get; }

    public Guid SectionId { get; }

    public Guid ReleaseId { get; }

    public ReleaseContentBlockUnlockViewModel(Guid id, Guid sectionId, Guid releaseId)
    {
        Id = id;
        SectionId = sectionId;
        ReleaseId = releaseId;
    }
}