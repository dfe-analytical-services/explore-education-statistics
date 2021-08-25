#nullable enable
using System;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    public class MethodologyContentBlock : ContentBlock
    {
        public string? Body { get; set; }

        public MethodologyContentSection ContentSection { get; set; } = null!;

        public Guid ContentSectionId { get; set; }
    }
}
