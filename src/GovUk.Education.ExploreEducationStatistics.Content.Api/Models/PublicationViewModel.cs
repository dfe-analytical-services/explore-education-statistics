using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Models
{
    public class PublicationViewModel
    {
        public PublicationViewModel(Guid id,
            string title,
            string slug,
            string description,
            string dataSource,
            string summary,
            DateTime? nextUpdate,
            Guid latestReleaseId,
            List<ReleaseTitleViewModel> otherReleases,
            List<LinkViewModel> legacyReleases,
            TopicViewModel topic,
            ContactViewModel contact,
            string externalMethodology,
            MethodologySummaryViewModel methodology)
        {
            Id = id;
            Title = title;
            Slug = slug;
            Description = description;
            DataSource = dataSource;
            Summary = summary;
            NextUpdate = nextUpdate;
            LatestReleaseId = latestReleaseId;
            OtherReleases = otherReleases;
            LegacyReleases = legacyReleases;
            Topic = topic;
            Contact = contact;
            ExternalMethodology = externalMethodology;
            Methodology = methodology;
        }

        public Guid Id { get; set; }

        public string Title { get; set; }

        public string Slug { get; set; }

        public string Description { get; set; }

        public string DataSource { get; set; }

        public string Summary { get; set; }

        public DateTime? NextUpdate { get; set; }

        public Guid LatestReleaseId { get; set; }

        public List<ReleaseTitleViewModel> OtherReleases { get; set; }

        public List<LinkViewModel> LegacyReleases { get; set; }

        public TopicViewModel Topic { get; set; }

        public ContactViewModel Contact { get; set; }

        public string ExternalMethodology { get; set; }

        public MethodologySummaryViewModel Methodology { get; set; }
    }
}