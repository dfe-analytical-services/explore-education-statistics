#nullable enable
using System;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ManageContent;

public record ContentBlockUnlockViewModel(Guid Id, Guid SectionId, Guid ReleaseId);