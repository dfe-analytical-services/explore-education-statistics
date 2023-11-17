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
        public static Release CreateAmendment(
            this Release originalRelease,
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
            // EES-4637 - we need to decide on how we're being consistent with Created dates in Release Amendments.
            amendment.Created = createdDate;
            amendment.CreatedById = createdByUserId;
            amendment.Version = originalRelease.Version + 1;
            amendment.PreviousVersionId = originalRelease.Id;

            // Create new DataBlockVersions for each DataBlockParent and for each, replace the "LatestVersion"
            // with the new DataBlockVersion.
            amendment.DataBlockVersions = amendment
                .DataBlockVersions
                .Select(originalDataBlockVersion =>
                {
                    var clonedDataBlockVersion = originalDataBlockVersion.Clone(amendment);
                    clonedDataBlockVersion.DataBlockParent.LatestPublishedVersion = originalDataBlockVersion;
                    clonedDataBlockVersion.DataBlockParent.LatestPublishedVersionId = originalDataBlockVersion.Id;
                    clonedDataBlockVersion.DataBlockParent.LatestVersion = clonedDataBlockVersion;
                    clonedDataBlockVersion.DataBlockParent.LatestVersionId = clonedDataBlockVersion.Id;
                    return clonedDataBlockVersion;
                })
                .ToList();

            // Clone all non-DataBlock Content Blocks found within Release Content.
            Dictionary<ContentBlock, ContentBlock> originalToClonedContentBlocks = originalRelease
                .Content
                .SelectMany(section => section.Content)
                .Where(block => block is not DataBlock)
                .ToDictionary(
                    block => block,
                    block => block.Clone(amendment));

            var originalToClonedDataBlocks = amendment
                .DataBlockVersions
                .Select(dataBlockVersion => dataBlockVersion.DataBlockParent)
                .ToDictionary(
                    dataBlockParent => dataBlockParent.LatestPublishedVersion!.ContentBlock,
                    dataBlockParent => dataBlockParent.LatestVersion!.ContentBlock);

            var allClonedBlocks = originalToClonedContentBlocks
                .ToDictionary(kv => kv.Key, kv => kv.Value);

            originalToClonedDataBlocks.ForEach(kv => allClonedBlocks.Add(kv.Key, kv.Value));

            // Copy ContentSections, using the newly-cloned ContentBlocks and DataBlocks rather than
            // the original ones in the new ContentSections.
            amendment.Content = amendment
                .Content
                // Old / new ContentBlock pairs are added to context here.
                .Select(section => section.Clone(amendment, allClonedBlocks))
                .ToList();

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
                var amendmentKeyStatistic = originalKeyStatistic.Clone(amendment);

                if (originalKeyStatistic is KeyStatisticDataBlock originalKeyStatDataBlock
                    && amendmentKeyStatistic is KeyStatisticDataBlock amendmentKeyStatDataBlock)
                {
                    var amendmentDataBlock = originalToClonedDataBlocks[originalKeyStatDataBlock.DataBlock];
                    amendmentKeyStatDataBlock.DataBlock = amendmentDataBlock;
                    amendmentKeyStatDataBlock.DataBlockId = amendmentDataBlock.Id;
                }

                return amendmentKeyStatistic;
            }).ToList();

            amendment.FeaturedTables = amendment.FeaturedTables.Select(originalFeaturedTable =>
            {
                var amendmentFeaturedTable = originalFeaturedTable.Clone(amendment);

                var amendmentDataBlock = originalToClonedDataBlocks[originalFeaturedTable.DataBlock];
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

            UpdateAmendmentContent(allClonedBlocks);

            return amendment;
        }

        private static void UpdateAmendmentContent(Dictionary<ContentBlock, ContentBlock> allClonedBlocks)
        {
            var replacements = new Dictionary<string, MatchEvaluator>();

            var regex = new Regex(
                string.Join('|', replacements.Keys.Append(ContentFilterUtils.CommentsFilterPattern)),
                RegexOptions.Compiled
            );

            foreach (var amendmentBlock in allClonedBlocks.Values)
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
    }
}
