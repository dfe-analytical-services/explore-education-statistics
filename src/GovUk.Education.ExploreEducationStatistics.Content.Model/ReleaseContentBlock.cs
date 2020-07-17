using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    public class ReleaseContentBlock
    {
        public Release Release { get; set; }

        public Guid ReleaseId { get; set; }

        public ContentBlock ContentBlock { get; set; }

        public Guid ContentBlockId { get; set; }

        public ReleaseContentBlock Clone(CreateClonedContext ctx)
        {
            var copy = MemberwiseClone() as ReleaseContentBlock;
            
            copy.Release = ctx.Target;
            copy.ReleaseId = ctx.Target.Id;
            
            var newVersionOfContentBlock = 
                ctx.OldToNewIdContentBlockMappings.GetValueOrDefault(ContentBlock) 
                ?? ContentBlock.Clone(ctx, null);

            copy.ContentBlock = newVersionOfContentBlock;
            copy.ContentBlockId = newVersionOfContentBlock.Id;
            
            return copy;
        }
    }
}