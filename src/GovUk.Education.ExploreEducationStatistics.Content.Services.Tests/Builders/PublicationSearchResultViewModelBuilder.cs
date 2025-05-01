using System;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Tests.Builders;

public class PublicationSearchResultViewModelBuilder
{
    private string? _publicationSlug;

    public PublicationSearchResultViewModel Build()
    {
        return new PublicationSearchResultViewModel
        {
            Id = Guid.NewGuid(),
            Published = DateTime.UtcNow,
            Rank = 4,
            Slug = _publicationSlug ?? "publication-slug",
            LatestReleaseSlug = "latest-release-slug",
            Summary = "publication summary",
            Theme = "theme name",
            Title = "Publication Title",
            Type = ReleaseType.ExperimentalStatistics
        };
    }

    public PublicationSearchResultViewModelBuilder WithSlug(string publicationSlug)
    {
        _publicationSlug = publicationSlug;
        return this;
    }
}
