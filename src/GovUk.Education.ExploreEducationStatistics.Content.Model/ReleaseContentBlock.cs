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

        public ReleaseContentBlock Clone(Release newRelease, Release.CloneContext context)
        {
            var copy = MemberwiseClone() as ReleaseContentBlock;

            copy.Release = newRelease;
            copy.ReleaseId = newRelease.Id;

            var clonedContentBlock =
                context.ContentBlocks.GetValueOrDefault(ContentBlock)
                ?? ContentBlock.Clone(context, null);

            copy.ContentBlock = clonedContentBlock;
            copy.ContentBlockId = (Guid) clonedContentBlock.Id;

            return copy;
        }
    }
}