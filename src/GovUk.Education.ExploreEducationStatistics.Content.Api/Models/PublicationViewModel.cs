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
            Guid latestReleaseId,
            List<ReleaseTitleViewModel> otherReleases,
            List<LegacyReleaseViewModel> legacyReleases,
            TopicViewModel topic,
            ContactViewModel contact,
            ExternalMethodologyViewModel externalMethodology,
            MethodologySummaryViewModel methodology)
        {
            Id = id;
            Title = title;
            Slug = slug;
            Description = description;
            DataSource = dataSource;
            Summary = summary;
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

        public Guid LatestReleaseId { get; set; }

        public List<ReleaseTitleViewModel> OtherReleases { get; set; }

        public List<LegacyReleaseViewModel> LegacyReleases { get; set; }

        public TopicViewModel Topic { get; set; }

        public ContactViewModel Contact { get; set; }

        public ExternalMethodologyViewModel ExternalMethodology { get; set; }

        public MethodologySummaryViewModel Methodology { get; set; }
    }
}