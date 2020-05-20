using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class Subject
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        public IEnumerable<Observation> Observations { get; set; }
        public ICollection<SubjectFootnote> Footnotes { get; set; }
    }
}