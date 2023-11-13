using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    public enum ContentSectionType
    {
        Generic,
        ReleaseSummary,
        KeyStatisticsSecondary,
        Headlines,
        RelatedDashboards,
    }

    public class ContentSection
    {
        public Guid Id { get; set; }

        public int Order { get; set; }

        public string Heading { get; set; }

        public string Caption { get; set; }

        public List<ContentBlock> Content { get; set; } = new();

        public Release Release { get; set; }

        public Guid ReleaseId { get; set; }

        [JsonIgnore] public ContentSectionType Type { get; set; }

        // TODO EES-4639 - rewrite Release Amendment generation code to be localised to
        // ReleaseAmendmentExtensions.CreateAmendment() or ReleaseService.CreateBasicReleaseAmendment().
        public ContentSection Clone(Release.CloneContext context)
        {
            var copy = MemberwiseClone();
            copy.Id = Guid.NewGuid();

            copy.Release = context.NewRelease;
            copy.ReleaseId = context.NewRelease.Id;

            copy.Content = copy
                .Content?
                .Select(content => content.Clone(context, copy))
                .ToList();

            return copy;
        }

        // TODO EES-4639 - rewrite Methodology Amendment generation code to be localised to
        // MethodologyVersion.CreateMethodologyAmendment() or MethodologyAmendmentService.CreateAndSaveAmendment().
        public ContentSection Clone(DateTime createdDate)
        {
            var copy = MemberwiseClone();
            copy.Id = Guid.NewGuid();

            copy.Content = copy
                .Content?
                .Select(content => content.Clone(createdDate))
                .ToList();

            copy.Content?.ForEach(c => c.ContentSectionId = copy.Id);
            return copy;
        }

        // TODO EES-4639 - adopt straight MemberwiseClone() usage during the generating of Release Amendments,
        // Methodology Amendments and Releases from templates, then have each call site tailor the resulting
        // cloned Content tree to their individual requirements, rather than support various Clone() methods in
        // Release Content entities themselves.
        public new ContentSection MemberwiseClone()
        {
            return base.MemberwiseClone() as ContentSection;
        }
    }
}
