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

        public ReleaseContentSection Release { get; set; }

        [JsonIgnore] public ContentSectionType Type { get; set; }

        public ContentSection Clone(ReleaseContentSection newReleaseContentSection, Release.CloneContext context)
        {
            var copy = MemberwiseClone() as ContentSection;
            copy.Id = Guid.NewGuid();

            copy.Release = newReleaseContentSection;

            copy.Content = copy
                .Content?
                .Select(content => content.Clone(context, copy))
                .ToList();

            return copy;
        }

        public ContentSection Clone(DateTime createdDate)
        {
            var copy = MemberwiseClone() as ContentSection;
            copy.Id = Guid.NewGuid();

            copy.Content = copy
                .Content?
                .Select(content => content.Clone(createdDate))
                .ToList();

            copy.Content?.ForEach(c => c.ContentSectionId = copy.Id);
            return copy;
        }
    }
}
