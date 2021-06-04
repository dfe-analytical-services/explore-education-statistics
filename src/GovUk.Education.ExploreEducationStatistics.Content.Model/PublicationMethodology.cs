using System;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    public class PublicationMethodology
    {
        public Publication Publication { get; set; }

        public Guid PublicationId { get; set; }

        public MethodologyParent MethodologyParent { get; set; }

        public Guid MethodologyParentId { get; set; }

        public bool Owner { get; set; }
    }
}
