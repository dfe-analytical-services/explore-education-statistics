#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels
{
    public record PublicationSummaryViewModel
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Slug { get; set; } = string.Empty;

        public PublicationSummaryViewModel()
        {
        }

        public PublicationSummaryViewModel(CachedPublicationViewModel publication)
        {
            Id = publication.Id;
            Title = publication.Title;
            Slug = publication.Slug;
        }
    }
}
