#nullable enable
using System;
using System.Collections.Generic;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseContentSectionType;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    public enum ReleaseContentSectionType
    {
        Generic,
        ReleaseSummary,
        KeyStatistics,
        KeyStatisticsSecondary,
        Headlines
    }

    public class ReleaseContentSection : ContentSection
    {
        public List<ReleaseContentBlock> Content { get; set; } = new();

        public ReleaseContentSectionType Type { get; set; } = Generic;
        
        public Release Release { get; set; } = null!;

        public Guid ReleaseId { get; set; }

        public ReleaseContentSection Clone(Release newRelease, Release.CloneContext context)
        {
            var copy = MemberwiseClone() as ReleaseContentSection;
            // copy.Release = newRelease;
            // copy.ReleaseId = newRelease.Id;
            //
            // var contentSection = copy
            //     .ContentSection
            //     .Clone(copy, context);
            //
            // copy.ContentSection = contentSection;
            // copy.ContentSectionId = contentSection.Id;

            return copy;
        }
    }
}
