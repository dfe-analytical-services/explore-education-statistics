using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class Observation
    {
        public long Id { get; set; }
        public Subject Subject { get; set; }
        public long SubjectId { get; set; }
        public GeographicLevel GeographicLevel { get; set; }
        public Location Location { get; set; }
        public long LocationId { get; set; }
        public School School { get; set; }
        public string SchoolLaEstab { get; set; }
        public int Year { get; set; }
        public TimeIdentifier TimeIdentifier { get; set; }
        public Dictionary<long, string> Measures { get; set; }
        public ICollection<ObservationFilterItem> FilterItems { get; set; }
    }
}