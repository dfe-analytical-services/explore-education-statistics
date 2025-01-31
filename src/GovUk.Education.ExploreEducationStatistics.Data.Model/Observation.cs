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

        public Location Location { get; set; }

        public Guid LocationId { get; set; }

        public int Year { get; set; }

        public TimeIdentifier TimeIdentifier { get; set; }

        public Dictionary<Guid, string> Measures { get; set; }

        public List<ObservationFilterItem> FilterItems { get; set; } = new();

        public long CsvRow { get; set; }
    }
}
