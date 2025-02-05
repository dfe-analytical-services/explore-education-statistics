using System.Diagnostics.CodeAnalysis;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Content.ViewModels;

public record PublicationSummaryViewModel
{
    public required Guid Id { get; init; }

    public required string Title { get; init; }

    public required string Slug { get; init; }

    public string? LatestReleaseSlug { get; init; }

    public bool? Owner { get; init; }
    
    public ContactViewModel? Contact { get; init; }

    public PublicationSummaryViewModel()
    {
    }

    [SetsRequiredMembers]
    public PublicationSummaryViewModel(PublicationCacheViewModel publicationCache)
    {
        Id = publicationCache.Id;
        Title = publicationCache.Title;
        Slug = publicationCache.Slug;
    }

    [SetsRequiredMembers]
    public PublicationSummaryViewModel(Publication publication) : this(publication, hasContact: false)
    {
    }

    [SetsRequiredMembers]
    public PublicationSummaryViewModel(Publication publication, bool hasContact = false)
    {
        Id = publication.Id;
        Title = publication.Title;
        Slug = publication.Slug;
        Contact = hasContact ? new ContactViewModel(publication.Contact) : null;
    }
}
