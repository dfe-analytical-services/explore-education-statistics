using System;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    public class ReleaseContentSection
    {
        public Release Release { get; set; }

        public Guid ReleaseId { get; set; }

        public ContentSection ContentSection { get; set; }

        public Guid ContentSectionId { get; set; }

        public ReleaseContentSection Clone(CreateClonedContext ctx)
        {
            var copy = MemberwiseClone() as ReleaseContentSection;
            copy.Release = ctx.Target;
            copy.ReleaseId = ctx.Target.Id;

            var contentSection = copy
                .ContentSection
                .Clone(ctx, copy);

            copy.ContentSection = contentSection;
            copy.ContentSectionId = contentSection.Id;

            return copy;
        }
    }
}