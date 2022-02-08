using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels
{
    public class PublicationViewModel
    {
        public PublicationViewModel(
            Guid id,
            string title,
            string slug,
            Guid latestReleaseId,
            List<ReleaseTitleViewModel> otherReleases,
            List<LegacyReleaseViewModel> legacyReleases,
            TopicViewModel topic,
            ContactViewModel contact,
            ExternalMethodologyViewModel externalMethodology)
        {
            Id = id;
            Title = title;
            Slug = slug;
            LatestReleaseId = latestReleaseId;
            OtherReleases = otherReleases;
            LegacyReleases = legacyReleases;
            Topic = topic;
            Contact = contact;
            ExternalMethodology = externalMethodology;
        }

        public Guid Id { get; }

        public string Title { get; }

        public string Slug { get; }

        public Guid LatestReleaseId { get; }

        public List<ReleaseTitleViewModel> OtherReleases { get; }

        public List<LegacyReleaseViewModel> LegacyReleases { get; }

        public TopicViewModel Topic { get; }

        public ContactViewModel Contact { get; }

        public ExternalMethodologyViewModel ExternalMethodology { get; }

        public List<MethodologyVersionSummaryViewModel> Methodologies { get; set; }
    }
}
