#nullable enable
using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;

public record PublicationCacheViewModel
{
    public Guid Id { get; init; }

    public string Title { get; init; } = string.Empty;

    public string Slug { get; init; } = string.Empty;

    public Guid LatestReleaseId { get; init; }

    public bool IsSuperseded { get; init; }

    public List<ReleaseTitleViewModel> Releases { get; init; } = new();

    public List<LegacyReleaseViewModel> LegacyReleases { get; init; } = new();

    public TopicViewModel Topic { get; init; } = null!;

    public string? SupersededByTitle { get; init; }

    public string? SupersededBySlug { get; init; }

    public ContactViewModel Contact { get; init; } = null!;

    public ExternalMethodologyViewModel? ExternalMethodology { get; init; }
}