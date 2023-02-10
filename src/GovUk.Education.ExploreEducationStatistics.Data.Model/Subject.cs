using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class Subject
    {
        public Guid Id { get; set; }
        public List<Observation> Observations { get; set; }
        public List<Filter> Filters { get; set; }
        public List<IndicatorGroup> IndicatorGroups { get; set; }
        public List<SubjectFootnote> Footnotes { get; set; }
        public bool SoftDeleted { get; set; }
    }
}
