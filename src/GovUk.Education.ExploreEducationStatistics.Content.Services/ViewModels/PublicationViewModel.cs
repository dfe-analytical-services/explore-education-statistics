#nullable enable
using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;

public record PublicationViewModel
{
    public Guid Id { get; init; }

    public string Title { get; init; } = string.Empty;

    public string Slug { get; init; } = string.Empty;

    public Guid LatestReleaseId { get; init; }

    public bool IsSuperseded { get; init; }

    public PublicationSupersededByViewModel? SupersededBy { get; init; } = new();

    public List<ReleaseTitleViewModel> Releases { get; init; } = new();

    public List<LegacyReleaseViewModel> LegacyReleases { get; init; } = new();

    public TopicViewModel Topic { get; init; } = null!;

    public ContactViewModel Contact { get; init; } = null!;

    public ExternalMethodologyViewModel? ExternalMethodology { get; init; }

    public List<MethodologyVersionSummaryViewModel> Methodologies { get; init; } = new();

    public PublicationViewModel()
    {
    }

    public PublicationViewModel(PublicationCacheViewModel publication,
        List<MethodologyVersionSummaryViewModel> methodologySummaries)
    {
        Id = publication.Id;
        Title = publication.Title;
        Slug = publication.Slug;
        LatestReleaseId = publication.LatestReleaseId;
        IsSuperseded = publication.IsSuperseded;
        SupersededBy = publication.SupersededBy;
        Releases = publication.Releases;
        LegacyReleases = publication.LegacyReleases;
        Topic = publication.Topic;
        Contact = publication.Contact;
        ExternalMethodology = publication.ExternalMethodology;
        Methodologies = methodologySummaries;
    }
}