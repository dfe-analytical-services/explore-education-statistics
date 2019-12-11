using System;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Model
{
    public class ValidateReleaseMessage
    {
        public string PublicationSlug { get; set; }
        public Guid ReleaseId { get; set; }
        public string ReleaseSlug { get; set; }
        public DateTime PublishScheduled { get; set; }
    }
}