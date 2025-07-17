using System;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Tests.Builders;

public class PublicationCacheViewModelBuilder
{
    private string? _slug;

    public PublicationCacheViewModel Build()
    {
        return new PublicationCacheViewModel
        {
            Id = Guid.NewGuid(),
            Slug = _slug ?? "publication-slug"
        };
    }

    public PublicationCacheViewModelBuilder WithSlug(string slug)
    {
        _slug = slug;
        return this;
    }
}
