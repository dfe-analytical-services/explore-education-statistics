using System;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class BoundaryLevel
    {
        public long Id { get; set; }
        public GeographicLevel Level { get; set; }
        public string Label { get; set; }
        public DateTime Published { get; set; }
    }
}