#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Utils;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions
{
    public static class ReleaseAmendmentExtensions
    {
        public static Tuple<Release, List<DataBlock>> CreateAmendment(
            this Release originalRelease,
            List<DataBlock> originalDataBlocks,
            DateTime createdDate,
            Guid createdByUserId)
        {
            var amendment = originalRelease.Clone();

            // Set new values for fields that should be altered in the amended
            // Release rather than copied from the original Release
            amendment.Id = Guid.NewGuid();
            amendment.Published = null;
            amendment.PublishScheduled = null;
            amendment.ApprovalStatus = ReleaseApprovalStatus.Draft;
            amendment.NotifiedOn = null;
            amendment.NotifySubscribers = false;
            amendment.UpdatePublishedDate = false;
            amendment.Created = createdDate;
            amendment.CreatedById = createdByUserId;
            amendment.Version = originalRelease.Version + 1;
            amendment.PreviousVersionId = originalRelease.Id;

            var context = new Release.CloneContext(amendment);

            // Copy ContentSections and ContentBlocks associated with those sections
            amendment.Content = amendment
                .Content
                // Old / new ContentBlock pairs are added to context here.
                .Select(section => section.Clone(context))
                .ToList();

            // TODO EES-4467 - this can be incorporated back into Release as the newly fashioned
            // DataBlock / DataBlockVersions tables.
            // Copy DataBlocks not associated with any ContentSection.
            // DataBlocks copied previously are fetched from context so not cloned twice.
            originalDataBlocks
                // Only clone DataBlocks that have not already been cloned via Content.
                .Where(dataBlock => !context.OriginalToAmendmentContentBlockMap.ContainsKey(dataBlock))
                // Old / new DataBlock pairs are added to context here.
                .ForEach(dataBlock => dataBlock.Clone(context));

            // NOTE: This is to ensure that a RelatedDashboards ContentSection exists on all new amendments.
            // There are older releases without a RelatedDashboards ContentSection.
            if (!amendment
                    .Content
                    .Any(c => c is { Type: ContentSectionType.RelatedDashboards }))
            {
                amendment.Content.Add(new ContentSection
                {
                    Id = Guid.NewGuid(),
                    Type = ContentSectionType.RelatedDashboards,
                    Content = new List<ContentBlock>(),
                    Release = amendment,
                    ReleaseId = amendment.Id
                });
            }

            amendment.KeyStatistics = amendment.KeyStatistics.Select(originalKeyStatistic =>
            {
                var amendmentKeyStatistic = originalKeyStatistic.Clone(context.NewRelease);

                if (originalKeyStatistic is KeyStatisticDataBlock originalKeyStatDataBlock
                    && amendmentKeyStatistic is KeyStatisticDataBlock amendmentKeyStatDataBlock)
                {
                    var originalDataBlock = originalKeyStatDataBlock.DataBlock;
                    var amendmentDataBlock = (DataBlock)context.OriginalToAmendmentContentBlockMap[originalDataBlock];
                    amendmentKeyStatDataBlock.DataBlock = amendmentDataBlock;
                    amendmentKeyStatDataBlock.DataBlockId = amendmentDataBlock.Id;
                }

                return amendmentKeyStatistic;
            }).ToList();

            amendment.FeaturedTables = amendment.FeaturedTables.Select(originalFeaturedTable =>
            {
                var amendmentFeaturedTable = originalFeaturedTable.Clone(context.NewRelease);

                var originalDataBlock = originalFeaturedTable.DataBlock;
                var amendmentDataBlock = (DataBlock)context.OriginalToAmendmentContentBlockMap[originalDataBlock];
                amendmentFeaturedTable.DataBlock = amendmentDataBlock;
                amendmentFeaturedTable.DataBlockId = amendmentDataBlock.Id;

                return amendmentFeaturedTable;
            }).ToList();

            amendment.RelatedInformation = amendment
                .RelatedInformation
                .Select(link => link.Clone())
                .ToList();

            amendment.Updates = amendment
                .Updates
                .Select(update => update.Clone(amendment))
                .ToList();

            UpdateAmendmentContent(context);

            // TODO EES-4467 - the list of DataBlocks can be incorporated back into Release as the newly fashioned
            // DataBlock / DataBlockVersion tables and this Tuple can be removed at that point.
            return new Tuple<Release, List<DataBlock>>(
                amendment, 
                context
                    .OriginalToAmendmentContentBlockMap
                    .Values
                    .OfType<DataBlock>()
                    .ToList());
        }

        private static void UpdateAmendmentContent(Release.CloneContext context)
        {
            var replacements = new Dictionary<string, MatchEvaluator>();

            // Bit cheeky to re-use the clone context, but it's a nice
            // easy way to access and modify all of the content blocks
            // that we used during the clone.
            context.OriginalToAmendmentContentBlockMap
                .ForEach(
                    pair =>
                    {
                        switch (pair)
                        {
                            case { Key: DataBlock oldDataBlock, Value: DataBlock newDataBlock }:
                                replacements[$"/fast-track/{oldDataBlock.Id}"] = _ => $"/fast-track/{newDataBlock.Id}";
                                break;
                        }
                    }
                );

            var regex = new Regex(
                string.Join('|', replacements.Keys.Append(ContentFilterUtils.CommentsFilterPattern)),
                RegexOptions.Compiled
            );

            foreach (var amendmentBlock in context.OriginalToAmendmentContentBlockMap.Values)
            {
                switch (amendmentBlock)
                {
                    case HtmlBlock block:
                        block.Body = ReplaceContent(block.Body, regex, replacements);
                        break;

                    case MarkDownBlock block:
                        block.Body = ReplaceContent(block.Body, regex, replacements);
                        break;

                    case EmbedBlockLink block:
                        block.EmbedBlock = block.EmbedBlock.Clone();
                        break;
                }
            }
        }

        private static string ReplaceContent(
            string content,
            Regex regex,
            Dictionary<string, MatchEvaluator> replacements)
        {
            if (content.IsNullOrEmpty())
            {
                return content;
            }

            return regex.Replace(
                content,
                match =>
                {
                    if (replacements.ContainsKey(match.Value))
                    {
                        return replacements[match.Value](match);
                    }

                    // Assume that it is a filtered match and we should remove it.
                    return string.Empty;
                }
            );
        }

        public static void CreateGenericContentFromTemplate(
            this Release originalRelease, 
            Release.CloneContext context)
        {
            var newRelease = context.NewRelease;
            
            newRelease.Content = originalRelease.Content
                .Where(section => section.Type == ContentSectionType.Generic)
                .ToList();

            newRelease.Content = originalRelease
                .Content
                .Select(section => section.Clone(context))
                .ToList();

            // TODO EES-4467 - we can add the cloning of Data Blocks back in here rather than being doing separately
            // when the new DataBlock / DataBlockVersion tables are added.
        }
    }
}
