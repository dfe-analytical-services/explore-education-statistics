using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class Observation
    {
        public long Id { get; set; }
        public Subject Subject { get; set; }
        public long SubjectId { get; set; }
        public Level Level { get; set; }
        public Location Location { get; set; }
        public long LocationId { get; set; }
        public School School { get; set; }
        public long SchoolId { get; set; }
        public int Year { get; set; }
        public TimePeriod TimePeriod { get; set; }
        public IEnumerable<ObservationFilterItem> FilterItems { get; set; }
        public IEnumerable<Measure> Measures { get; set; }
    }
}