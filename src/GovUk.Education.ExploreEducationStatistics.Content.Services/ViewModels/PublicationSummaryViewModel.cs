#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels
{
    public record PublicationSummaryViewModel
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Slug { get; set; } = string.Empty;
        
        public bool Owner { get; set; }
        
        public Contact? Contact { get; set; }

        public PublicationSummaryViewModel()
        {
        }

        public PublicationSummaryViewModel(PublicationCacheViewModel publication)
        {
            Id = publication.Id;
            Title = publication.Title;
            Slug = publication.Slug;
        }
    }
}
