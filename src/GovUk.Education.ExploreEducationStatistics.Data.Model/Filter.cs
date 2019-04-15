namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class Filter
    {
        public long Id { get; set; }
        public string Label { get; set; }
        public string Hint { get; set; }
        public FilterGroup FilterGroup { get; set; }
        public long FilterGroupId { get; set; }
    }
}