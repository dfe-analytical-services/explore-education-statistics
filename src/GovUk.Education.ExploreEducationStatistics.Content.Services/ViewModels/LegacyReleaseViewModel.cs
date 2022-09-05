#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;

public record LegacyReleaseViewModel(Guid Id, string Description, string Url)
{
    public LegacyReleaseViewModel(LegacyRelease legacyRelease) : this(
        Id: legacyRelease.Id,
        Description: legacyRelease.Description,
        Url: legacyRelease.Url
    )
    {
    }
}
