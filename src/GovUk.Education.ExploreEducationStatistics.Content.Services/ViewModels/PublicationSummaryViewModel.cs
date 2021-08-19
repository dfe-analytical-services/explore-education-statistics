using System;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels
{
    public class PublicationSummaryViewModel
    {
        public Guid Id { get; }

        public string Title { get; }

        public string Slug { get;  }

        public string Description { get; }

        public string DataSource { get; }

        public string Summary { get; }

        public PublicationSummaryViewModel(CachedPublicationViewModel publication)
        {
            Id = publication.Id;
            Title = publication.Title;
            Slug = publication.Slug;
            Description = publication.Description;
            DataSource = publication.DataSource;
            Summary = publication.Summary;
        }
    }
}