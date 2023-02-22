#nullable enable
using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class Subject
    {
        public Guid Id { get; set; }
        public List<Observation> Observations { get; set; } = new();
        public List<Filter> Filters { get; set; } = new();
        public List<IndicatorGroup> IndicatorGroups { get; set; } = new();
        public List<SubjectFootnote> Footnotes { get; set; } = new();
        public bool SoftDeleted { get; set; }
    }
}
