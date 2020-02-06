using System;

namespace GovUk.Education.ExploreEducationStatistics.Common.Model.Data
{
    public class BoundaryLevel
    {
        public long Id { get; set; }
        public GeographicLevel Level { get; set; }
        public string Label { get; set; }
        public DateTime Published { get; set; }
    }
}