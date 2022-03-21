using System;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    public class ReleaseContentSection
    {
        public Release Release { get; set; }

        public Guid ReleaseId { get; set; }

        public ContentSection ContentSection { get; set; }

        public Guid ContentSectionId { get; set; }

        public ReleaseContentSection Clone(Release.CloneContext context)
        {
            var copy = MemberwiseClone() as ReleaseContentSection;
            copy.Release = context.NewRelease;
            copy.ReleaseId = context.NewRelease.Id;

            var contentSection = copy
                .ContentSection
                .Clone(copy, context);

            copy.ContentSection = contentSection;
            copy.ContentSectionId = contentSection.Id;

            return copy;
        }
    }
}