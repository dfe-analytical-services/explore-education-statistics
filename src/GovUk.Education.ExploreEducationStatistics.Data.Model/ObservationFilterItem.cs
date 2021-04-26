using System;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class ObservationFilterItem
    {
        public Observation Observation { get; set; }
        public Guid ObservationId { get; set; }
        public FilterItem FilterItem { get; set; }
        public Guid FilterItemId { get; set; }
        public Filter Filter { get; set; }
        public Guid? FilterId { get; set; }
    }
}