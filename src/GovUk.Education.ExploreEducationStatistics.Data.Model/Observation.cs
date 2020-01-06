using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class Observation
    {
        public Guid Id { get; set; }
        public Subject Subject { get; set; }
        public Guid SubjectId { get; set; }
        public GeographicLevel GeographicLevel { get; set; }
        public Location Location { get; set; }
        public Guid LocationId { get; set; }
        public Provider Provider { get; set; }
        public string ProviderUrn { get; set; }
        public School School { get; set; }
        public string SchoolLaEstab { get; set; }
        public int Year { get; set; }
        public TimeIdentifier TimeIdentifier { get; set; }
        public Dictionary<Guid, string> Measures { get; set; }
        public ICollection<ObservationFilterItem> FilterItems { get; set; }
        public long CsvRow { get; set; }
    }
}