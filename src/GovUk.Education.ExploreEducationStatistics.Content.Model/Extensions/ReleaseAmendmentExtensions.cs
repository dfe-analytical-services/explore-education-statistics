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
                .Select(rcs => rcs.Clone(context))
                .ToList();

            // NOTE: This is to ensure that a RelatedDashboards ContentSection exists on all new amendments.
            // There are older releases without a RelatedDashboards ContentSection.
            if (!amendment.Content
                    .Any(c => c.ContentSection is { Type: ContentSectionType.RelatedDashboards }))
            {
                amendment.Content.Add(new ReleaseContentSection
                {
                    Release = amendment,
                    ReleaseId = amendment.Id,
                    ContentSection = new ContentSection
                    {
                        Id = Guid.NewGuid(),
                        Type = ContentSectionType.RelatedDashboards,
                        Content = new List<ContentBlock>(),
                    }
                });
            }

            amendment.ContentBlocks = amendment
                .ContentBlocks
                .Select(rcb => rcb.Clone(context))
                .ToList();

            amendment.RelatedInformation = amendment
                .RelatedInformation
                .Select(link => link.Clone())
                .ToList();

            amendment.Updates = amendment
                .Updates
                .Select(update => update.Clone(amendment))
                .ToList();

            UpdateAmendmentContent(context);

            return amendment;
        }

        private static void UpdateAmendmentContent(Release.CloneContext context)
        {
            var replacements = new Dictionary<string, MatchEvaluator>();

            // Bit cheeky to re-use the clone context, but it's a nice
            // easy way to access and modify all of the content blocks
            // that we used during the clone.
            context.ContentBlocks
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

            foreach (var contentBlock in context.ContentBlocks.Values)
            {
                switch (contentBlock)
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
