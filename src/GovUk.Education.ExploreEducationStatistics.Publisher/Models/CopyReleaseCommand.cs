using System;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Models
{
    public class CopyReleaseCommand
    {
        public Guid ReleaseId { get; set; }
        public string PublicationSlug { get; set; }
        public string ReleaseSlug { get; set; }
        public DateTime PublishScheduled { get; set; }
    }
}