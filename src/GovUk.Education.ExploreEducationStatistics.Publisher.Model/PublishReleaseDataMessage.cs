using System;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Model
{
    public class PublishReleaseDataMessage
    {
        public string PublicationSlug { get; set; }
        public DateTime ReleasePublished { get; set; }
        public string ReleaseSlug { get; set; }
    }
}