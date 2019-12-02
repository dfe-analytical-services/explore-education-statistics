using System;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    public class ReleaseContentBlock
    {
        public Release Release { get; set; }

        public Guid ReleaseId { get; set; }

        public IContentBlock ContentBlock { get; set; }

        public Guid ContentBlockId { get; set; }
    }
}