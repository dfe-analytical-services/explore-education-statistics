#nullable enable
using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    public class MethodologyContentSection : ContentSection
    {
        public List<MethodologyContentBlock> Content { get; set; } = new();

        public Methodology Methodology { get; set; } = null!;

        public Guid MethodologyId { get; set; }
    }
}
