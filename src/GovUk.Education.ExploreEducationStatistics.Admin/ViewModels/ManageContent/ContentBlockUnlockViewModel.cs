#nullable enable
using System;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ManageContent;

public record ContentBlockUnlockViewModel
{
    public Guid Id { get; }

    public Guid SectionId { get; }

    public Guid ReleaseId { get; }

    public ContentBlockUnlockViewModel(Guid id, Guid sectionId, Guid releaseId)
    {
        Id = id;
        SectionId = sectionId;
        ReleaseId = releaseId;
    }
}