using System;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    public class ReleaseContentSection
    {
        public Release Release { get; set; }

        public Guid ReleaseId { get; set; }

        public ContentSection ContentSection { get; set; }

        public Guid ContentSectionId { get; set; }
    }
}