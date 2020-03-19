using System;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    public class ReleaseContentSection
    {
        public Release Release { get; set; }

        public Guid ReleaseId { get; set; }

        public ContentSection ContentSection { get; set; }

        public Guid ContentSectionId { get; set; }

        public ReleaseContentSection CreateReleaseAmendment(CreateAmendmentContext ctx)
        {
            var copy = MemberwiseClone() as ReleaseContentSection;
            copy.Release = ctx.Amendment;
            copy.ReleaseId = ctx.Amendment.Id;
            
            copy.ContentSection = copy
                .ContentSection
                .CreateReleaseAmendment(ctx, copy);
            
            return copy;
        }
    }
}