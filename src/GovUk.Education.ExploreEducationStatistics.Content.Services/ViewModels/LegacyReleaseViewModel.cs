#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;

public record LegacyReleaseViewModel
{
    public LegacyReleaseViewModel()
    {
    }

    public LegacyReleaseViewModel(LegacyRelease legacyRelease)
    {
        Id = legacyRelease.Id;
        Description = legacyRelease.Description;
        Url = legacyRelease.Url;
    }

    public Guid Id { get; init; }

    public string Description { get; init; } = string.Empty;

    public string Url { get; init; } = string.Empty;
}
