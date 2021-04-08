using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;

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
        public int Year { get; set; }
        public TimeIdentifier TimeIdentifier { get; set; }
        public Dictionary<Guid, string> Measures { get; set; }
        public ICollection<ObservationFilterItem> FilterItems { get; set; }
        public long CsvRow { get; set; }
    }
}