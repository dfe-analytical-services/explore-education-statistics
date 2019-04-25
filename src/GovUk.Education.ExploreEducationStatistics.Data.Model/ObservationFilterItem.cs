namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class ObservationFilterItem
    {
        public Observation Observation { get; set; }
        public long ObservationId { get; set; }
        public FilterItem FilterItem { get; set; }
        public long FilterItemId { get; set; }
    }
}