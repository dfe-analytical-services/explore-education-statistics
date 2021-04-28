using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class Subject
    {
        public Guid Id { get; set; }
        public ICollection<Observation> Observations { get; set; }
        public ICollection<Filter> Filters { get; set; }
        public ICollection<IndicatorGroup> IndicatorGroups { get; set; }
        public ICollection<SubjectFootnote> Footnotes { get; set; }
        public bool SoftDeleted { get; set; }
    }
}
