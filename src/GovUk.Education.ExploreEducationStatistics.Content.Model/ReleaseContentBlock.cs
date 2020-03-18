using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    public class ReleaseContentBlock
    {
        public Release Release { get; set; }

        public Guid ReleaseId { get; set; }

        public IContentBlock ContentBlock { get; set; }

        public Guid ContentBlockId { get; set; }

        public ReleaseContentBlock CreateReleaseAmendment(CreateAmendmentContext ctx)
        {
            var copy = MemberwiseClone() as ReleaseContentBlock;
            
            copy.Release = ctx.Amendment;
            copy.ReleaseId = ctx.Amendment.Id;
            
            var newVersionOfContentBlock = 
                ctx.OldToNewIdContentBlockMappings.GetValueOrDefault(ContentBlock) 
                ?? ContentBlock.CreateReleaseAmendment(ctx, null);

            copy.ContentBlock = newVersionOfContentBlock;
            copy.ContentBlockId = newVersionOfContentBlock.Id;
            
            return copy;
        }
    }
}