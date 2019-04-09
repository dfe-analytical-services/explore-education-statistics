namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Meta
{
    public class IndicatorMeta : IMeta
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Label { get; set; }
        public string Group { get; set; }
        public Unit Unit { get; set; }
        public bool KeyIndicator { get; set; }
        public Subject Subject { get; set; }
        public long SubjectId { get; set; }
    }
}