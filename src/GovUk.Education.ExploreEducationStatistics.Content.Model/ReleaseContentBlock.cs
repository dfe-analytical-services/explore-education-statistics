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

        public ReleaseContentBlock Clone(Release.CloneContext context)
        {
            var copy = MemberwiseClone() as ReleaseContentBlock;

            copy.Release = context.NewRelease;
            copy.ReleaseId = context.NewRelease.Id;

            var clonedContentBlock =
                context.OriginalToAmendmentContentBlockMap.GetValueOrDefault(ContentBlock)
                ?? ContentBlock.Clone(context);

            copy.ContentBlock = clonedContentBlock;
            copy.ContentBlockId = clonedContentBlock.Id;

            return copy;
        }
    }
}
