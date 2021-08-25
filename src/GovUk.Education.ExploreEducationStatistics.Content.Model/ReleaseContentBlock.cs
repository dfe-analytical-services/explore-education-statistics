#nullable enable
using System;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    public class ReleaseContentBlock : ContentBlock
    {
        public ReleaseContentSection ContentSection { get; set; } = null!;

        public Guid ContentSectionId { get; set; }

        public string? Body { get; set; }

        public Guid? DataBlockId { get; set; }

        public DataBlock? DataBlock { get; set; }

        public ReleaseContentBlock Clone(Release newRelease, Release.CloneContext context)
        {
            var copy = MemberwiseClone() as ReleaseContentBlock;

            // copy.Release = newRelease;
            // copy.ReleaseId = newRelease.Id;
            //
            // var clonedContentBlock =
            //     context.ContentBlocks.GetValueOrDefault(ContentBlock)
            //     ?? ContentBlock.Clone(context, null);
            //
            // copy.ContentBlock = clonedContentBlock;
            // copy.ContentBlockId = (Guid) clonedContentBlock.Id;

            return copy;
        }
    }
}
