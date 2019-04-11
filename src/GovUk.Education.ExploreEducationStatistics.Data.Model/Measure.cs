namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class Measure
    {
        public long Id { get; set; }
        public string Value { get; set; }
        public Indicator Indicator { get; set; }
        public long IndicatorId { get; set; }
        public Observation Observation { get; set; }
        public long ObservationId { get; set; }
    }
}