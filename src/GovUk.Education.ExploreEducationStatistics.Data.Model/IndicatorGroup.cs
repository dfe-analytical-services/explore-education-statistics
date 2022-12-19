#nullable enable
using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class IndicatorGroup
    {
        public Guid Id { get; set; }
        public string Label { get; set; } = string.Empty;
        public Subject Subject { get; set; } = null!;
        public Guid SubjectId { get; set; }
        public List<Indicator> Indicators { get; set; } = new();

        public IndicatorGroup()
        {
        }

        public IndicatorGroup(string label, Guid subjectId)
        {
            Id = Guid.NewGuid();
            Label = label;
            SubjectId = subjectId;
            Indicators = new List<Indicator>();
        }
    }
}
