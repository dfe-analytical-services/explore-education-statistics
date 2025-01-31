using System;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    public class PublicationMethodology
    {
        public Publication Publication { get; set; }

        public Guid PublicationId { get; set; }

        public Methodology Methodology { get; set; }

        public Guid MethodologyId { get; set; }

        public bool Owner { get; set; }
    }
}
