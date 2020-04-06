using System;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Model
{
    public class PublishReleaseContentImmediateMessage
    {
        public Guid ReleaseId { get; set; }
        public Guid ReleaseStatusId { get; set; }
    }
}