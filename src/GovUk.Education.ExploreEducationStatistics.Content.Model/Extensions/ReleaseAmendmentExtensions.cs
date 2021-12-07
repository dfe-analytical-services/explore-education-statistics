#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions
{
    public static class ReleaseAmendmentExtensions
    {
        public static Release CreateAmendment(this Release release, DateTime createdDate, Guid createdByUserId)
        {
            var amendment = release.Clone();

            // Set new values for fields that should be altered in the amended
            // Release rather than copied from the original Release
            amendment.Id = Guid.NewGuid();
            amendment.Published = null;
            amendment.PublishScheduled = null;
            amendment.ApprovalStatus = ReleaseApprovalStatus.Draft;
            amendment.Created = createdDate;
            amendment.CreatedById = createdByUserId;
            amendment.Version = release.Version + 1;
            amendment.PreviousVersionId = release.Id;

            var context = new Release.CloneContext(amendment);

            amendment.Content = amendment
                .Content
                ?.Select(rcs => rcs.Clone(context))
                .ToList();

            amendment.ContentBlocks = amendment
                .ContentBlocks
                ?.Select(rcb => rcb.Clone(context))
                .ToList();

            amendment.RelatedInformation = amendment
                .RelatedInformation
                ?.Select(link => link.Clone())
                .ToList();

            amendment.Updates = amendment
                .Updates
                ?.Select(update => update.Clone(amendment))
                .ToList();

            UpdateAmendmentContent(context);

            return amendment;
        }

        private static void UpdateAmendmentContent(Release.CloneContext context)
        {
            // Bit cheeky to re-use the clone context, but it's a nice
            // easy way to access and modify all of the content blocks
            // that we used during the clone.
            var dataBlocks = context.ContentBlocks
                .Where(pair => pair.Key is DataBlock && pair.Value is DataBlock)
                .ToDictionary(
                    pair => pair.Key as DataBlock,
                    pair => pair.Value as DataBlock
                ) as Dictionary<DataBlock, DataBlock>;

            foreach (var contentBlock in context.ContentBlocks.Values)
            {
                switch (contentBlock)
                {
                    case HtmlBlock block:
                        block.Body = UpdateFastTrackLinks(block.Body, dataBlocks);
                        break;
                    case MarkDownBlock block:
                        block.Body = UpdateFastTrackLinks(block.Body, dataBlocks);
                        break;
                }
            }
        }

        private static string UpdateFastTrackLinks(string content, Dictionary<DataBlock, DataBlock> dataBlocks)
        {
            if (content.IsNullOrEmpty())
            {
                return content;
            }

            var nextContent = content;

            foreach (var (oldDataBlock, newDataBlock) in dataBlocks)
            {
                // Not a particularly fast way to replace fast tracks
                // if there's a lot of content. Could be improved
                // using something like a parallel substring approach.
                nextContent = nextContent.Replace(
                    $"/fast-track/{oldDataBlock.Id}",
                    $"/fast-track/{newDataBlock.Id}"
                );
            }

            return nextContent;
        }

        public static void CreateGenericContentFromTemplate(this Release release, Release newRelease)
        {
            var context = new Release.CloneContext(newRelease);

            newRelease.Content = release.Content
                .Where(c => c.ContentSection.Type == ContentSectionType.Generic)
                .ToList();

            newRelease.Content = newRelease
                .Content
                .Select(rcs => rcs.Clone(context))
                .ToList();

            newRelease.ContentBlocks = newRelease
                .ContentBlocks
                .Select(rcb => rcb.Clone(context))
                .ToList();
        }
    }
}