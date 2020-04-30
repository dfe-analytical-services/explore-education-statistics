using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class Subject
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        
        // TODO BAU-384 - remove after migration endpoint run on all environments
        public Release Release { get; set; }
        
        // TODO BAU-384 - remove after migration endpoint run on all environments
        public Guid ReleaseId { get; set; }
        
        public IEnumerable<Observation> Observations { get; set; }
        public ICollection<SubjectFootnote> Footnotes { get; set; }
    }
}