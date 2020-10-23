using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.ViewModels
{
    public class PublicationViewModel
    {
        public PublicationViewModel(
            Guid id,
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

        public Guid Id { get; }

        public string Title { get; }

        public string Slug { get; }

        public string Description { get; }

        public string DataSource { get; }

        public string Summary { get; }

        public Guid LatestReleaseId { get; }

        public List<ReleaseTitleViewModel> OtherReleases { get; }

        public List<LegacyReleaseViewModel> LegacyReleases { get; }

        public TopicViewModel Topic { get; }

        public ContactViewModel Contact { get; }

        public ExternalMethodologyViewModel ExternalMethodology { get; }

        public MethodologySummaryViewModel Methodology { get; }
    }
}